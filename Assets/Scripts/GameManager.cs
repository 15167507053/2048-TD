﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//游戏状态 用于配合动画效果
public enum GameState
{
    Playing,                //可操作
    GameSuspension,         //不接受输入
    WaitingForMoveToEnd,     //等待移动结束
}

//定义一个枚举来储存方块元素类型
public enum ElementType
{
    Empty = -1, //空

    //可移动的单位
    Player = 0, //主角
    Enemy,      //敌人
    Material,   //建材
    TowerEnemy,     //远程型敌人
    BuilderEnemy,   //造墙型敌人

    //不可移动的单位
    Tower,      //攻击塔
    Power,      //发电站
    Mall,       //商场
    Wall,       //防御塔
    Landmine,   //地雷
    Trap,       //陷阱【】
    Refuge,     //避难所
    Magnetic,   //干扰磁场【】

    //不可移动且不可拆的单位
    Access,     //主角进入避难所
    AssistedEnemy,  //支援型敌人
}

///主控流程
public class GameManager : MonoBehaviour
{
    public GameState State;     //游戏状态

    public bool won = false;   //游戏是否已经取得胜利

    private bool move = false; //玩家是否发生过移动
    private int turn = 0;      //记录回合数
    private bool RefugeTurn = false;    //记录是否在本回合进入避难所
    private bool BuffTurn = false;      //记录是否在本回合被添加了buff

    #region 行列与方块列表

    //用于获取所有的方块 
    public Tile[,] AllTiles = new Tile[11, 8];

    //创建行和列的列表 用于移动
    private List<Tile[]> colums = new List<Tile[]>();
    private List<Tile[]> rows = new List<Tile[]>();

    //用于获取空方快 在产生新方块时用于标识
    private List<Tile> EmptyTiles = new List<Tile>();

    #endregion

    /// 1.新游戏开始时的初始化
    void Start()
    {
        //新游戏开始
        //over = true;    //游戏处于开启状态
        State = GameState.Playing;
        won = false;    //本关还未取得胜利
        move = false;
        turn = 3;       //从第3回合开始（为了在三回合后 产生第一个敌人
        RefugeTurn = false;    //还未进入避难所
        BuffTurn = false;
        EventManager.Instance.LCount = 0;      //地雷还未建造过

        //游戏开始时清除场地
        Tile[] AllTilesOneDim = GameObject.FindObjectsOfType<Tile>();   //获取到所有的方块
        foreach (Tile t in AllTilesOneDim)
        {
            t.TileType = ElementType.Empty;     //游戏开始时清除场地 ，清除所有方块上的信息
            AllTiles[t.indRow, t.indCol] = t;   //在游戏管理器中存储关于所有图块的信息 ，在每一个方块上都储存其行号和列号
            EmptyTiles.Add(t);                  //将当前位置存入空图块列表

            //if(t.indRow == 0 && t.indCol == 0)
            //{
            //    Generate(ElementType.Player, t.indRow, t.indCol);   //在指定位置创建主角
            //}

        }

        # region 初始化 行和列 的列表
        colums.Add(new Tile[] { AllTiles[0, 0], AllTiles[1, 0], AllTiles[2, 0], AllTiles[3, 0], AllTiles[4, 0], AllTiles[5, 0], AllTiles[6, 0], AllTiles[7, 0], AllTiles[8, 0], AllTiles[9, 0], AllTiles[10, 0] });
        colums.Add(new Tile[] { AllTiles[0, 1], AllTiles[1, 1], AllTiles[2, 1], AllTiles[3, 1], AllTiles[4, 1], AllTiles[5, 1], AllTiles[6, 1], AllTiles[7, 1], AllTiles[8, 1], AllTiles[9, 1], AllTiles[10, 1] });
        colums.Add(new Tile[] { AllTiles[0, 2], AllTiles[1, 2], AllTiles[2, 2], AllTiles[3, 2], AllTiles[4, 2], AllTiles[5, 2], AllTiles[6, 2], AllTiles[7, 2], AllTiles[8, 2], AllTiles[9, 2], AllTiles[10, 2] });
        colums.Add(new Tile[] { AllTiles[0, 3], AllTiles[1, 3], AllTiles[2, 3], AllTiles[3, 3], AllTiles[4, 3], AllTiles[5, 3], AllTiles[6, 3], AllTiles[7, 3], AllTiles[8, 3], AllTiles[9, 3], AllTiles[10, 3] });
        colums.Add(new Tile[] { AllTiles[0, 4], AllTiles[1, 4], AllTiles[2, 4], AllTiles[3, 4], AllTiles[4, 4], AllTiles[5, 4], AllTiles[6, 4], AllTiles[7, 4], AllTiles[8, 4], AllTiles[9, 4], AllTiles[10, 4] });
        colums.Add(new Tile[] { AllTiles[0, 5], AllTiles[1, 5], AllTiles[2, 5], AllTiles[3, 5], AllTiles[4, 5], AllTiles[5, 5], AllTiles[6, 5], AllTiles[7, 5], AllTiles[8, 5], AllTiles[9, 5], AllTiles[10, 5] });
        colums.Add(new Tile[] { AllTiles[0, 6], AllTiles[1, 6], AllTiles[2, 6], AllTiles[3, 6], AllTiles[4, 6], AllTiles[5, 6], AllTiles[6, 6], AllTiles[7, 6], AllTiles[8, 6], AllTiles[9, 6], AllTiles[10, 6] });
        colums.Add(new Tile[] { AllTiles[0, 7], AllTiles[1, 7], AllTiles[2, 7], AllTiles[3, 7], AllTiles[4, 7], AllTiles[5, 7], AllTiles[6, 7], AllTiles[7, 7], AllTiles[8, 7], AllTiles[9, 7], AllTiles[10, 7] });

        rows.Add(new Tile[] { AllTiles[0, 0], AllTiles[0, 1], AllTiles[0, 2], AllTiles[0, 3], AllTiles[0, 4], AllTiles[0, 5], AllTiles[0, 6], AllTiles[0, 7] });
        rows.Add(new Tile[] { AllTiles[1, 0], AllTiles[1, 1], AllTiles[1, 2], AllTiles[1, 3], AllTiles[1, 4], AllTiles[1, 5], AllTiles[1, 6], AllTiles[1, 7] });
        rows.Add(new Tile[] { AllTiles[2, 0], AllTiles[2, 1], AllTiles[2, 2], AllTiles[2, 3], AllTiles[2, 4], AllTiles[2, 5], AllTiles[2, 6], AllTiles[2, 7] });
        rows.Add(new Tile[] { AllTiles[3, 0], AllTiles[3, 1], AllTiles[3, 2], AllTiles[3, 3], AllTiles[3, 4], AllTiles[3, 5], AllTiles[3, 6], AllTiles[3, 7] });
        rows.Add(new Tile[] { AllTiles[4, 0], AllTiles[4, 1], AllTiles[4, 2], AllTiles[4, 3], AllTiles[4, 4], AllTiles[4, 5], AllTiles[4, 6], AllTiles[4, 7] });
        rows.Add(new Tile[] { AllTiles[5, 0], AllTiles[5, 1], AllTiles[5, 2], AllTiles[5, 3], AllTiles[5, 4], AllTiles[5, 5], AllTiles[5, 6], AllTiles[5, 7] });
        rows.Add(new Tile[] { AllTiles[6, 0], AllTiles[6, 1], AllTiles[6, 2], AllTiles[6, 3], AllTiles[6, 4], AllTiles[6, 5], AllTiles[6, 6], AllTiles[6, 7] });
        rows.Add(new Tile[] { AllTiles[7, 0], AllTiles[7, 1], AllTiles[7, 2], AllTiles[7, 3], AllTiles[7, 4], AllTiles[7, 5], AllTiles[7, 6], AllTiles[7, 7] });
        rows.Add(new Tile[] { AllTiles[8, 0], AllTiles[8, 1], AllTiles[8, 2], AllTiles[8, 3], AllTiles[8, 4], AllTiles[8, 5], AllTiles[8, 6], AllTiles[8, 7] });
        rows.Add(new Tile[] { AllTiles[9, 0], AllTiles[9, 1], AllTiles[9, 2], AllTiles[9, 3], AllTiles[9, 4], AllTiles[9, 5], AllTiles[9, 6], AllTiles[9, 7] });
        rows.Add(new Tile[] { AllTiles[10, 0], AllTiles[10, 1], AllTiles[10, 2], AllTiles[10, 3], AllTiles[10, 4], AllTiles[10, 5], AllTiles[10, 6], AllTiles[10, 7] });
        #endregion

        //开局时新建【1个主角】【2个建材】【x个墙壁】
        Generate(ElementType.Player);
        Generate(ElementType.Material);
        Generate(ElementType.Material);
        //Generate(ElementType.Enemy);

        int x = 3;
        while (x > 0)
        {
            Generate(ElementType.Wall);
            x--;
        }
    }

    #region 2.建造一个指定类型的单位
    //【随机位置建造】 参数是需要建造的单位类型
    public void Generate(ElementType type)
    {
        //若场上仍然存在空方快
        if (EmptyTiles.Count > 0)
        {
            //随机一个位置
            int PosIndex = Random.Range(0, EmptyTiles.Count);

            //如果这个位置已经有东西了 则不进行建造
            if (EmptyTiles[PosIndex].TileType != ElementType.Empty)
            {
                EmptyTiles.RemoveAt(PosIndex);  //删除这个位置
                Generate(type);                 //重新触发建造
            }
            else
            {
                //如果这是敌人或建材单位 赋予其等级
                if (type == ElementType.Enemy || type == ElementType.Material)
                {
                    EmptyTiles[PosIndex].TileLevel = 2;
                }
                //其他情况清空等级
                else
                {
                    EmptyTiles[PosIndex].TileLevel = 0;
                }

                //将指定位置处的方块标记为 指定元素
                EmptyTiles[PosIndex].TileType = type;

                //从空方快列表中删除该位置
                EmptyTiles.RemoveAt(PosIndex);
            }
        }
    }

    //【指定位置建造】 参数是需要建造的单位【类型】和【坐标】
    public bool Generate(ElementType type, int y, int x)
    {
        //若当前坐标为空
        if (AllTiles[x, y].TileType == ElementType.Empty)
        {
            //如果出现的是敌人或建材单位 赋予其等级
            if (type == ElementType.Enemy || type == ElementType.Material)
            {
                AllTiles[x, y].TileLevel = 2;
            }
            else
            {
                AllTiles[x, y].TileLevel = 0;
            }

            //将指定位置处的方块标记为 指定元素
            AllTiles[x, y].TileType = type;

            //从空方快列表中随便删一个位置
            //EmptyTiles.RemoveAt(Random.Range(0, EmptyTiles.Count));

            return true;
        }
        return false;
    }
    #endregion

    //获取到输入后的【回合行为】
    public void Move(MoveDirection md)
    {
        //if (over)   //若游戏已经结束 不执行回合的行为
        if (State == GameState.Playing)
        {
            /// 3.获取输入并处理
            bool moveMade = false;  //用于判断本次输入是否有效
            ResetMergedFlags();     //清空所有方块上的开关

            #region 触发【4.移动】【5.合并】【6.消耗】方法
            switch (md)
            {
                case MoveDirection.Down:    //下
                    for (int i = 0; i < colums.Count; i++)
                    {
                        while (MakeOneMoveDownIndex(colums[i]))   //i为行/列号 逐行传递给移动函数
                        {
                            moveMade = true;
                        }
                    }
                    break;
                case MoveDirection.Left:    //左
                    for (int i = 0; i < rows.Count; i++)
                    {
                        while (MakeOneMoveDownIndex(rows[i]))
                        {
                            moveMade = true;
                        }
                    }
                    break;
                case MoveDirection.Right:   //右
                    for (int i = 0; i < rows.Count; i++)
                    {
                        while (MakeOneMoveUpIndex(rows[i]))
                        {
                            moveMade = true;
                        }
                    }
                    break;
                case MoveDirection.Up:     //上
                    for (int i = 0; i < colums.Count; i++)
                    {
                        while (MakeOneMoveUpIndex(colums[i]))
                        {
                            moveMade = true;
                        }
                    }
                    break;
            }
            #endregion

            //移动发生后的行为
            if (moveMade)
            {
                /// 7. 建筑行为 并更新空方快列表
                UpdateEmptyTiles();     //触发建筑行为 并且更新空方快列表 防止已有方块被覆盖

                #region 8.产生新的建材与敌人
                Generate(ElementType.Material);     //回合结束后 新建一个建材

                if ((turn % 15 == 0))
                {
                    Generate(ElementType.AssistedEnemy);    //每15个回合出现一个支援兵

                    if (turn > 40)
                    {
                        Generate(ElementType.BuilderEnemy); //40回合后出现造墙兵
                    }
                }
                else if (turn % 5 == 0)
                {
                    Generate(ElementType.Enemy);    //每五回合产生一个敌人

                    if (turn > 30)
                    {
                        Generate(ElementType.TowerEnemy);   //30回合后开始出现远程兵
                    }
                }

                if (turn % 10 == 0)
                {
                    Generate(ElementType.Wall);     //每十回合产生一个墙
                }
                #endregion

                #region 9.游戏胜负的判定
                //【30回合后】场上【不存在敌人】 则游戏胜利 但仅限于第一次获得胜利的情况
                if (!won && turn > 32 && NotEnemy())    //turn从3开始
                {
                    PanelManager.Instance.YouWon();
                }

                //如果电力不足
                if (Power.Instance.Numerical <= 0)
                {
                    //GameOver("电力不足");    //显示游戏结束消息
                    PanelManager.Instance.GameOver("<size=20> 電力が足りない </size>\n", " 建材が４つ揃ったら、すぐに発電所を建てましょう");
                }
                //或是没有可移动的方块
                //else if (!CanMove())
                //{
                //    //GameOver("没有可移动的方块");
                //    PanelManager.Instance.GameOver("移動できるコマがない");
                //}
                #endregion

                /// 0.回合结束
                turn++;
                move = false;
                RefugeTurn = false;
                BuffTurn = false;
            }
        }
    }

    //【移动】与【合并】方块 参数是【指定行号的行/列】
    bool MakeOneMoveDownIndex(Tile[] LineOfTiles)   //下 左
    {
        //在逐行获得行/列后，逐个判断方块，进行移动与合并
        for (int i = 0; i < LineOfTiles.Length - 1; i++)
        {
            #region 4.Move Block 移动方块

            //减速buff
            if (LineOfTiles[i + 1].SlowBuff && !BuffTurn)
            {
                //只移动一格
                LineOfTiles[i].TileLevel = LineOfTiles[i + 1].TileLevel;    //将后方方块的等级转移到自己身上
                LineOfTiles[i].TileType = LineOfTiles[i + 1].TileType;      //将其的类型也进行转移
                LineOfTiles[i + 1].TileLevel = 0;                           //清除遗留的数字
                LineOfTiles[i + 1].TileType = ElementType.Empty;            //清除自身

                LineOfTiles[i].mergedThisTurn = true;   //不再移动
                LineOfTiles[i + 1].SlowBuff = false;    //解除buff
                LineOfTiles[i].moveThisTurn = true;     //本格发生过移动
            }

            //若方块【自身为空】，且后方有一个【非空】&【非建筑】&【没有发生过碰撞】的方块
            else if (LineOfTiles[i].TileType == ElementType.Empty && LineOfTiles[i + 1].TileType != ElementType.Empty &&
                LineOfTiles[i + 1].TileType < ElementType.Tower && LineOfTiles[i + 1].mergedThisTurn == false)
            {
                //通用的移动代码
                LineOfTiles[i].TileLevel = LineOfTiles[i + 1].TileLevel;    //将后方方块的等级转移到自己身上
                LineOfTiles[i].TileType = LineOfTiles[i + 1].TileType;      //将其的类型也进行转移
                LineOfTiles[i + 1].TileLevel = 0;                           //清除遗留的数字

                //造墙兵留下墙
                if (LineOfTiles[i + 1].TileType == ElementType.BuilderEnemy)
                {
                    LineOfTiles[i + 1].TileType = ElementType.Wall;
                }
                //普通单位清空足迹
                else
                {
                    LineOfTiles[i + 1].TileType = ElementType.Empty;            //清空后方方块的数值
                }

                //主角或敌人移动时进行消耗
                if (LineOfTiles[i].TileType == ElementType.Player || LineOfTiles[i].TileType == ElementType.Enemy)
                {
                    Power.Instance.Numerical -= 1;
                }
                LineOfTiles[i].moveThisTurn = true;  //本格（该单位）发生过移动
                return true;    //用于控制循环，直至没有可合并的方块
            }

            //若上一格方块为满员避难所 放出主角
            else if (LineOfTiles[i + 1].TileType == ElementType.Access && RefugeTurn == false)
            {
                //如果本格为空 将主角放置于此位置
                if (LineOfTiles[i].TileType == ElementType.Empty)
                {
                    //放出主角
                    LineOfTiles[i].TileType = ElementType.Player;
                    //将自身改回普通避难所
                    LineOfTiles[i + 1].TileType = ElementType.Refuge;
                    //本格发生过移动
                    LineOfTiles[i].moveThisTurn = true;
                }
            }

            #endregion

            #region 5.Merge Block 合并方块
            //碰撞规则
            if (LineOfTiles[i].TileType != ElementType.Empty)   //若方块【自身不为空】
            {
                //根据自身类型选择合并规则（自身是被合并的一方
                switch (LineOfTiles[i].TileType)
                {
                    #region 主角
                    case ElementType.Player:
                        //检查自身是否发生过移动
                        if (LineOfTiles[i].moveThisTurn)
                        {
                            move = true;
                        }
                        //遇到敌人时被破坏 但前提是对方本回合并【未发生过碰撞 & 发生过移动】
                        if (LineOfTiles[i + 1].TileType == ElementType.Enemy &&
                            LineOfTiles[i + 1].mergedThisTurn == false /*&& LineOfTiles[i + 1].moveThisTurn == true*/)
                        {
                            //GameOver("玩家受到攻击"); //游戏失败
                            PanelManager.Instance.GameOver("<size=20> プレーヤーが攻撃された </size>\n", " 移動の際、主人公の後ろに敵がいるのかとうかを確かめたほうがいいよ");
                            return false; ;
                        }
                        //不与其他单位发生事件
                        break;
                    #endregion

                    #region 建材
                    case ElementType.Material:
                        //同等级同类 且自身与后方【本回合都未发生过合并】 合并升级
                        if (LineOfTiles[i + 1].TileType == ElementType.Material && LineOfTiles[i].TileLevel == LineOfTiles[i + 1].TileLevel &&
                            LineOfTiles[i].mergedThisTurn == false && LineOfTiles[i + 1].mergedThisTurn == false)
                        {
                            LineOfTiles[i].TileLevel *= 2;      //等级翻倍
                            LineOfTiles[i].UpdateTile();        //更新方块内容
                            LineOfTiles[i + 1].TileLevel = 0;   //清空前一个方块的等级
                            LineOfTiles[i + 1].TileType = ElementType.Empty;    //清空前一个方块的样式
                            LineOfTiles[i].mergedThisTurn = true;   //该方块不再合并
                            return true;
                        }
                        //主角 被吸收并提供相应资源 且自身与后方【本回合都未发生过合并】
                        else if (LineOfTiles[i + 1].TileType == ElementType.Player &&
                            LineOfTiles[i].mergedThisTurn == false && LineOfTiles[i + 1].mergedThisTurn == false)
                        {
                            Material.Instance.Numerical += LineOfTiles[i].TileLevel;    //获取到相应资源
                            LineOfTiles[i].TileType = ElementType.Player;       //将自身销毁 改为主角
                            LineOfTiles[i + 1].TileLevel = 0;                   //清空前一个方块的数字
                            LineOfTiles[i + 1].TileType = ElementType.Empty;    //清空前一个方块的样式
                            LineOfTiles[i].mergedThisTurn = true;               //不再合并
                            return true;
                        }
                        //敌人 被破坏 但是敌人必须发生过移动 且在自身发生过合并后可以免疫敌人的攻击
                        else if (LineOfTiles[i + 1].TileType == ElementType.Enemy && /*LineOfTiles[i + 1].moveThisTurn == true &&*/
                            LineOfTiles[i].mergedThisTurn == false && LineOfTiles[i + 1].mergedThisTurn == false)
                        {
                            //改变样式
                            LineOfTiles[i].TileLevel = LineOfTiles[i + 1].TileLevel;    //获取到敌人的等级
                            LineOfTiles[i].TileType = ElementType.Enemy;                //销毁自身 变为敌人
                            //清空上一个方块
                            LineOfTiles[i + 1].TileLevel = 0;
                            LineOfTiles[i + 1].TileType = ElementType.Empty;
                            //不再合并
                            LineOfTiles[i].mergedThisTurn = true;
                            return true;
                        }
                        //其他情况不发生事件
                        break;
                    #endregion

                    #region 敌人
                    case ElementType.Enemy:
                        //同等级同类 合并升级 且自身与后方【本回合都未发生过合并】
                        if (LineOfTiles[i + 1].TileType == ElementType.Enemy && LineOfTiles[i].TileLevel == LineOfTiles[i + 1].TileLevel &&
                            LineOfTiles[i].mergedThisTurn == false && LineOfTiles[i + 1].mergedThisTurn == false)
                        {
                            LineOfTiles[i].TileLevel *= 2;      //等级翻倍
                            LineOfTiles[i].UpdateTile();        //更新样式
                            LineOfTiles[i + 1].TileLevel = 0;   //清空上一个方块的等级
                            LineOfTiles[i + 1].TileType = ElementType.Empty;    //清空上一个方块的样式
                            LineOfTiles[i].mergedThisTurn = true;
                            return true;
                        }
                        //被其他单位碰撞不发生事件
                        break;

                    case ElementType.TowerEnemy:
                        //远程兵 仅移动 以及在回合结束时发动攻击 无其他事件
                        break;

                    case ElementType.BuilderEnemy:
                        //造墙兵 仅在移动时造墙 无其他事件
                        break;

                    case ElementType.AssistedEnemy:
                        //辅助兵 不移动 仅在回合结束时发生事件
                        //break;

                        //可以被主角破坏 前提是主角没有发生过碰撞
                        if (LineOfTiles[i + 1].TileType == ElementType.Player && LineOfTiles[i + 1].mergedThisTurn == false)
                        {
                            LineOfTiles[i].TileType = LineOfTiles[i + 1].TileType;  //改变自身样式
                            LineOfTiles[i + 1].TileType = ElementType.Empty;        //清空上一格
                            LineOfTiles[i].mergedThisTurn = true;       //关闭合并开关
                            return true;                                //停止移动
                        }
                        break;

                    #endregion

                    #region 地雷
                    case ElementType.Landmine:
                        #region 旧版地雷
                        //仅与敌人发生事件 【不管敌人处于何种状态】
                        //if (LineOfTiles[i + 1].TileType == ElementType.Enemy || LineOfTiles[i + 1].TileType == ElementType.Wall)
                        //{
                        //    //被8级以下敌人碰撞 毁灭敌人和自身
                        //    if (LineOfTiles[i + 1].TileLevel <= 8)
                        //    {
                        //        LineOfTiles[i].TileType = ElementType.Empty;    //销毁自身
                        //        LineOfTiles[i + 1].TileLevel = 0;               //清空敌人的等级
                        //        LineOfTiles[i + 1].TileType = ElementType.Empty;//清空敌人的样式
                        //    }
                        //    //8级以上敌人等级减半
                        //    else
                        //    {
                        //        LineOfTiles[i].TileLevel = LineOfTiles[i + 1].TileLevel / 2;    //等级减半
                        //        LineOfTiles[i].TileType = ElementType.Enemy;                    //销毁变为敌人
                        //        LineOfTiles[i + 1].TileLevel = 0;                               //清空上一个方块的等级
                        //        LineOfTiles[i + 1].TileType = ElementType.Empty;                //清空上一个方块的样式
                        //        LineOfTiles[i].mergedThisTurn = true;   //敌人不再碰撞
                        //    }
                        //    //不再移动
                        //    return true;
                        //}
                        #endregion

                        #region 新版地雷
                        //一回合只炸一个单位 且无视空方快的情况
                        if (LineOfTiles[i].mergedThisTurn == false && LineOfTiles[i + 1].TileType != ElementType.Empty)
                        {
                            //如果炸到主角 游戏结束
                            if (LineOfTiles[i + 1].TileType == ElementType.Player)
                            {
                                PanelManager.Instance.GameOver("<size=20> プレーヤーが爆発された</size> \n", " 自分が埋めた地雷に踏まないように");
                            }
                            //8级以下的任何物体都直接秒杀 自身不小毁(什么都能炸
                            else if (LineOfTiles[i + 1].TileLevel <= 8)
                            {
                                LineOfTiles[i + 1].TileLevel = 0;               //清空对方的等级
                                LineOfTiles[i + 1].TileType = ElementType.Empty;//清空对方的样式
                                LineOfTiles[i].mergedThisTurn = true;           //本回合停止爆炸
                            }
                            //8级以上单位自身被销毁 对方等级减半
                            else
                            {
                                LineOfTiles[i].TileLevel = LineOfTiles[i + 1].TileLevel / 2;    //等级减半
                                LineOfTiles[i].TileType = LineOfTiles[i + 1].TileType;          //销毁自身变为对方
                                LineOfTiles[i + 1].TileLevel = 0;                               //清空上一个方块的等级
                                LineOfTiles[i + 1].TileType = ElementType.Empty;                //清空上一个方块的样式
                                LineOfTiles[i].mergedThisTurn = true;                           //对方本回合不再碰撞
                            }
                        }
                        #endregion
                        break;
                    #endregion

                    #region 墙壁
                    case ElementType.Wall:
                        //不发生任何事件
                        break;

                        //推墙
                        if (LineOfTiles[i + 1].TileType != ElementType.Empty)
                        {
                            //可以被主角推动 但每回合【当前不处于边缘】 & 【只移动一格】&【主角未发生过合并】&【下一格为空】
                            if (LineOfTiles[i + 1].TileType == ElementType.Player && i - 1 >= 0 &&
                                LineOfTiles[i].moveThisTurn == false && LineOfTiles[i + 1].mergedThisTurn == false && LineOfTiles[i - 1].TileType == ElementType.Empty)
                            {
                                //将自身传递到前方一格
                                LineOfTiles[i - 1].TileType = ElementType.Wall;
                                //将主角移动到身后
                                LineOfTiles[i].TileType = ElementType.Player;
                                //清空主角身后的位置
                                LineOfTiles[i + 1].TileType = ElementType.Empty;

                                //关闭自身的移动开关
                                LineOfTiles[i - 1].moveThisTurn = true;
                            }
                        }
                        break;
                    #endregion

                    #region 陷阱
                    case ElementType.Trap:
                        //所有经过陷阱的单位都会被停下 且下回合都只能移动一步
                        if (LineOfTiles[i + 1].TileType < ElementType.Tower && LineOfTiles[i + 1].TileType != ElementType.Empty && LineOfTiles[i + 1].mergedThisTurn == false)
                        {
                            //添加缓速移动buff
                            LineOfTiles[i].SlowBuff = true;
                            BuffTurn = true;

                            //获取到对方的信息并销毁自身
                            LineOfTiles[i].TileLevel = LineOfTiles[i + 1].TileLevel;    //获取对方的等级
                            LineOfTiles[i].TileType = LineOfTiles[i + 1].TileType;      //获取对方的样式
                            LineOfTiles[i + 1].TileLevel = 0;                           //清空上一格的等级
                            LineOfTiles[i + 1].TileType = ElementType.Empty;            //清空上一格的样式

                            //使其停下并关闭对方的合并开关
                            LineOfTiles[i].mergedThisTurn = true;
                            return true;

                        }
                        break;
                    #endregion

                    #region 避难所
                    case ElementType.Refuge:
                        //只有主角可以进入 进入需要花费金钱（钱不足时无法进入）
                        if (LineOfTiles[i + 1].TileType == ElementType.Player && Money.Instance.Numerical >= 10)
                        {
                            //更改自身样式为进入状态
                            LineOfTiles[i].TileType = ElementType.Access;
                            //清空上一格的内容
                            LineOfTiles[i + 1].TileType = ElementType.Empty;
                            //扣除入场费
                            Money.Instance.Numerical -= 10;
                            //记录进入避难所的回合
                            RefugeTurn = true;
                        }

                        //空避难所会被敌人破坏
                        else if (LineOfTiles[i + 1].TileType == ElementType.Enemy &&
                                LineOfTiles[i + 1].mergedThisTurn == false /*&& LineOfTiles[i + 1].moveThisTurn == true*/)
                        {
                            //改变样式
                            LineOfTiles[i].TileLevel = LineOfTiles[i + 1].TileLevel;    //获取到敌人的等级
                            LineOfTiles[i].TileType = ElementType.Enemy;                //自身变为敌人
                            //清空上一个方块
                            LineOfTiles[i + 1].TileLevel = 0;
                            LineOfTiles[i + 1].TileType = ElementType.Empty;
                            //不再合并与移动
                            LineOfTiles[i].mergedThisTurn = true;
                            return true;
                        }

                        break;

                    //避难状态
                    case ElementType.Access:
                        //不在进入避难所的回合放出主角
                        //if (EnterRefuge == false)
                        //{
                        //    //如果下一格为空 将主角放置于该位置
                        //    if (i - 1 >= 0 && LineOfTiles[i - 1].TileType == ElementType.Empty)
                        //    {
                        //        //放出主角
                        //        LineOfTiles[i - 1].TileType = ElementType.Player;
                        //        //将自身改回普通避难所
                        //        LineOfTiles[i].TileType = ElementType.Refuge;
                        //    }
                        //}

                        //此外不接受任何碰撞（同墙
                        break;
                    #endregion

                    #region 其他建筑（攻击塔、发电站、商场
                    default:
                        //会被敌人破坏 但敌人本回合必须【进行过移动】&【未发生过合并】
                        if (LineOfTiles[i + 1].TileType == ElementType.Enemy &&
                            LineOfTiles[i + 1].mergedThisTurn == false /*&& LineOfTiles[i + 1].moveThisTurn == true*/)
                        {
                            //改变样式
                            LineOfTiles[i].TileLevel = LineOfTiles[i + 1].TileLevel;    //获取到敌人的等级
                            LineOfTiles[i].TileType = ElementType.Enemy;                //自身变为敌人
                            //LineOfTiles[i].UpdateTile();                                //更新方块的样式
                            //清空上一个方块
                            LineOfTiles[i + 1].TileLevel = 0;
                            LineOfTiles[i + 1].TileType = ElementType.Empty;
                            //不再合并与移动
                            LineOfTiles[i].mergedThisTurn = true;
                            return true;
                        }
                        break;
                        #endregion
                }
                //switch结束
                //return true;    //用于控制循环，直至没有可合并的方块
            }
            #endregion

        }

        //若检测不到可移动/合并的方块，则终止循环
        return false;
    }
    bool MakeOneMoveUpIndex(Tile[] LineOfTiles)     //上 右
    {
        for (int i = LineOfTiles.Length - 1; i > 0; i--)
        {
            #region 4.移动方块

            //减速buff
            if (LineOfTiles[i - 1].SlowBuff && !BuffTurn)
            {
                //只移动一格
                LineOfTiles[i].TileLevel = LineOfTiles[i - 1].TileLevel;    //将后方方块的等级转移到自己身上
                LineOfTiles[i].TileType = LineOfTiles[i - 1].TileType;      //将其的类型也进行转移
                LineOfTiles[i - 1].TileLevel = 0;                           //清除遗留的数字
                LineOfTiles[i - 1].TileType = ElementType.Empty;            //清除自身

                LineOfTiles[i].mergedThisTurn = true;   //不再移动
                LineOfTiles[i - 1].SlowBuff = false;    //解除buff
                LineOfTiles[i].moveThisTurn = true;     //本格发生过移动
            }

            //正常移动
            else if (LineOfTiles[i].TileType == ElementType.Empty && LineOfTiles[i - 1].TileType != ElementType.Empty &&
                LineOfTiles[i - 1].TileType < ElementType.Tower && LineOfTiles[i - 1].mergedThisTurn == false)
            {
                LineOfTiles[i].TileLevel = LineOfTiles[i - 1].TileLevel;
                LineOfTiles[i].TileType = LineOfTiles[i - 1].TileType;
                LineOfTiles[i - 1].TileLevel = 0;

                //造墙兵
                if (LineOfTiles[i - 1].TileType == ElementType.BuilderEnemy)
                {
                    LineOfTiles[i - 1].TileType = ElementType.Wall;
                }
                //普通单位
                else
                {
                    LineOfTiles[i - 1].TileType = ElementType.Empty;
                }

                //移动消耗
                if (LineOfTiles[i].TileType == ElementType.Player || LineOfTiles[i].TileType == ElementType.Enemy)
                {
                    Power.Instance.Numerical -= 1;
                }
                LineOfTiles[i].moveThisTurn = true;
                return true;
            }

            //避难所
            else if (LineOfTiles[i - 1].TileType == ElementType.Access && RefugeTurn == false)
            {
                if (LineOfTiles[i].TileType == ElementType.Empty)
                {
                    LineOfTiles[i].TileType = ElementType.Player;
                    LineOfTiles[i - 1].TileType = ElementType.Refuge;
                    LineOfTiles[i].moveThisTurn = true;
                }
            }

            #endregion

            #region 5.合并方块
            //碰撞规则
            if (LineOfTiles[i].TileType != ElementType.Empty)
            {
                switch (LineOfTiles[i].TileType)
                {
                    #region 主角
                    case ElementType.Player:
                        //检查自身是否发生移动
                        if (LineOfTiles[i].moveThisTurn)
                        {
                            move = true;
                        }
                        //遇到敌人时被破坏
                        if (LineOfTiles[i - 1].TileType == ElementType.Enemy &&
                            LineOfTiles[i - 1].mergedThisTurn == false /*&& LineOfTiles[i - 1].moveThisTurn == true*/)
                        {
                            //GameOver("玩家受到攻击");
                            PanelManager.Instance.GameOver("<size=20> プレーヤーが攻撃された </size>\n", " 移動の際、主人公の後ろに敵がいるのかとうかを確かめたほうがいいよ");
                            return false; ;
                        }
                        break;
                    #endregion

                    #region 建材
                    case ElementType.Material:
                        //同等级同类 合并升级
                        if (LineOfTiles[i - 1].TileType == ElementType.Material && LineOfTiles[i].TileLevel == LineOfTiles[i - 1].TileLevel &&
                            LineOfTiles[i].mergedThisTurn == false && LineOfTiles[i - 1].mergedThisTurn == false)
                        {
                            LineOfTiles[i].TileLevel *= 2;
                            LineOfTiles[i].UpdateTile();
                            LineOfTiles[i - 1].TileLevel = 0;
                            LineOfTiles[i - 1].TileType = ElementType.Empty;
                            LineOfTiles[i].mergedThisTurn = true;
                            return true;
                        }
                        //主角 被吸收并提供相应资源
                        else if (LineOfTiles[i - 1].TileType == ElementType.Player &&
                            LineOfTiles[i].mergedThisTurn == false && LineOfTiles[i - 1].mergedThisTurn == false)
                        {
                            Material.Instance.Numerical += LineOfTiles[i].TileLevel;
                            LineOfTiles[i].TileType = ElementType.Player;
                            LineOfTiles[i - 1].TileLevel = 0;
                            LineOfTiles[i - 1].TileType = ElementType.Empty;
                            LineOfTiles[i].mergedThisTurn = true;
                            return true;
                        }
                        //敌人 被破坏
                        else if (LineOfTiles[i - 1].TileType == ElementType.Enemy && /*LineOfTiles[i - 1].moveThisTurn == true &&*/
                            LineOfTiles[i].mergedThisTurn == false && LineOfTiles[i - 1].mergedThisTurn == false)
                        {
                            LineOfTiles[i].TileLevel = LineOfTiles[i - 1].TileLevel;
                            LineOfTiles[i].TileType = ElementType.Enemy;
                            LineOfTiles[i - 1].TileLevel = 0;
                            LineOfTiles[i - 1].TileType = ElementType.Empty;
                            LineOfTiles[i].mergedThisTurn = true;
                            return true;
                        }
                        break;
                    #endregion

                    #region 敌人
                    case ElementType.Enemy:
                        //同等级同类 合并升级
                        if (LineOfTiles[i - 1].TileType == ElementType.Enemy && LineOfTiles[i].TileLevel == LineOfTiles[i - 1].TileLevel &&
                            LineOfTiles[i].mergedThisTurn == false && LineOfTiles[i - 1].mergedThisTurn == false)
                        {
                            LineOfTiles[i].TileLevel *= 2;
                            LineOfTiles[i].UpdateTile();
                            LineOfTiles[i - 1].TileLevel = 0;
                            LineOfTiles[i - 1].TileType = ElementType.Empty;
                            LineOfTiles[i].mergedThisTurn = true;
                            return true;
                        }
                        break;

                    case ElementType.TowerEnemy:
                        //远程兵 仅移动 以及在回合结束时发动攻击 无其他事件
                        break;

                    case ElementType.BuilderEnemy:
                        //造墙兵 仅移动和造墙 无其他事件
                        break;

                    case ElementType.AssistedEnemy:
                        //辅助兵 不移动 仅在回合结束时发生事件
                        //break;

                        //可以被主角破坏 前提是主角没有发生过碰撞
                        if (LineOfTiles[i - 1].TileType == ElementType.Player && LineOfTiles[i - 1].mergedThisTurn == false)
                        {
                            LineOfTiles[i].TileType = LineOfTiles[i - 1].TileType;
                            LineOfTiles[i - 1].TileType = ElementType.Empty;
                            LineOfTiles[i].mergedThisTurn = true;
                            return true;
                        }
                        break;
                    #endregion

                    #region 地雷
                    case ElementType.Landmine:
                        #region 旧版地雷
                        //if (LineOfTiles[i - 1].TileType == ElementType.Enemy || LineOfTiles[i - 1].TileType == ElementType.Wall)
                        //{
                        //    if (LineOfTiles[i + 1].TileLevel <= 8)
                        //    {
                        //        LineOfTiles[i].TileType = ElementType.Empty;
                        //        LineOfTiles[i - 1].TileLevel = 0;
                        //        LineOfTiles[i - 1].TileType = ElementType.Empty;
                        //    }
                        //    else
                        //    {
                        //        LineOfTiles[i].TileLevel = LineOfTiles[i - 1].TileLevel / 2;
                        //        LineOfTiles[i].TileType = ElementType.Enemy;
                        //        LineOfTiles[i - 1].TileLevel = 0;
                        //        LineOfTiles[i - 1].TileType = ElementType.Empty;
                        //        LineOfTiles[i].mergedThisTurn = true;
                        //    }
                        //    return true;
                        //}
                        #endregion

                        #region 新版地雷
                        //一回合只炸一个单位
                        if (LineOfTiles[i].mergedThisTurn == false && LineOfTiles[i - 1].TileType != ElementType.Empty)
                        {
                            //如果炸到主角 游戏结束
                            if (LineOfTiles[i - 1].TileType == ElementType.Player)
                            {
                                PanelManager.Instance.GameOver("<size=20> プレーヤーが爆発された </size>\n", " 自分が埋めた地雷に踏まないように");
                            }
                            //8级以下单位直接炸死 自身不销毁
                            else if (LineOfTiles[i - 1].TileLevel <= 8)
                            {
                                LineOfTiles[i - 1].TileLevel = 0;
                                LineOfTiles[i - 1].TileType = ElementType.Empty;
                                LineOfTiles[i].mergedThisTurn = true;
                            }
                            //8级以上单位自身被销毁 对方等级减半
                            else
                            {
                                LineOfTiles[i].TileLevel = LineOfTiles[i - 1].TileLevel / 2;
                                LineOfTiles[i].TileType = LineOfTiles[i - 1].TileType;
                                LineOfTiles[i - 1].TileLevel = 0;
                                LineOfTiles[i - 1].TileType = ElementType.Empty;
                                LineOfTiles[i].mergedThisTurn = true;
                            }
                        }
                        #endregion
                        break;
                    #endregion

                    #region 墙壁
                    case ElementType.Wall:
                        //不发生任何事件
                        break;

                        //推墙
                        if (LineOfTiles[i - 1].TileType != ElementType.Empty)
                        {
                            //可以被主角推动 但每回合【当前不处于边缘】&【只移动一格】&【主角未发生过合并】&【后方为空】
                            if (LineOfTiles[i - 1].TileType == ElementType.Player && i + 1 < LineOfTiles.Length &&
                                LineOfTiles[i].moveThisTurn == false && LineOfTiles[i - 1].mergedThisTurn == false && LineOfTiles[i + 1].TileType == ElementType.Empty)
                            {
                                LineOfTiles[i + 1].TileType = ElementType.Wall;
                                LineOfTiles[i].TileType = ElementType.Player;
                                LineOfTiles[i - 1].TileType = ElementType.Empty;

                                LineOfTiles[i + 1].moveThisTurn = true;
                            }
                        }
                        break;
                    #endregion

                    #region 陷阱
                    case ElementType.Trap:
                        if (LineOfTiles[i - 1].TileType < ElementType.Tower && LineOfTiles[i - 1].TileType != ElementType.Empty && LineOfTiles[i - 1].mergedThisTurn == false)
                        {
                            LineOfTiles[i].SlowBuff = true;
                            BuffTurn = true;

                            LineOfTiles[i].TileLevel = LineOfTiles[i - 1].TileLevel;
                            LineOfTiles[i].TileType = LineOfTiles[i - 1].TileType;
                            LineOfTiles[i - 1].TileLevel = 0;
                            LineOfTiles[i - 1].TileType = ElementType.Empty;

                            LineOfTiles[i].mergedThisTurn = true;
                            return true;
                        }
                        break;
                    #endregion

                    #region 避难所
                    case ElementType.Refuge:
                        if (LineOfTiles[i - 1].TileType == ElementType.Player && Money.Instance.Numerical >= 10)
                        {
                            LineOfTiles[i].TileType = ElementType.Access;
                            LineOfTiles[i - 1].TileType = ElementType.Empty;
                            Money.Instance.Numerical -= 10;
                            RefugeTurn = true;
                        }
                        else if (LineOfTiles[i - 1].TileType == ElementType.Enemy &&
                                LineOfTiles[i - 1].mergedThisTurn == false /*&& LineOfTiles[i - 1].moveThisTurn == true*/)
                        {
                            LineOfTiles[i].TileLevel = LineOfTiles[i - 1].TileLevel;
                            LineOfTiles[i].TileType = ElementType.Enemy;
                            LineOfTiles[i - 1].TileLevel = 0;
                            LineOfTiles[i - 1].TileType = ElementType.Empty;
                            LineOfTiles[i].mergedThisTurn = true;
                            return true;
                        }

                        break;

                    case ElementType.Access:
                        //if (EnterRefuge == false)
                        //{
                        //    if (i + 1 < LineOfTiles.Length && LineOfTiles[i + 1].TileType == ElementType.Empty)
                        //    {
                        //        LineOfTiles[i + 1].TileType = ElementType.Player;
                        //        LineOfTiles[i].TileType = ElementType.Refuge;
                        //    }
                        //}
                        break;
                    #endregion

                    #region 其他建筑（攻击塔、发电站、商场
                    default:
                        //会被敌人破坏
                        if (LineOfTiles[i - 1].TileType == ElementType.Enemy &&
                            LineOfTiles[i - 1].mergedThisTurn == false /*&& LineOfTiles[i - 1].moveThisTurn == true*/)
                        {
                            LineOfTiles[i].TileLevel = LineOfTiles[i - 1].TileLevel;
                            LineOfTiles[i].TileType = ElementType.Enemy;
                            LineOfTiles[i - 1].TileLevel = 0;
                            LineOfTiles[i - 1].TileType = ElementType.Empty;
                            LineOfTiles[i].mergedThisTurn = true;
                            return true;
                        }
                        break;
                        #endregion
                }
            }
            #endregion
        }
        return false;
    }

    //管理合并开关（回合结束后
    private void ResetMergedFlags()
    {
        //遍历所有的方块
        foreach (Tile t in AllTiles)
        {
            t.mergedThisTurn = false;   //将他们标记为可合并，用于下回合的行动
            t.moveThisTurn = false;           //将所有的方块标记为未移动
        }
    }

    //计算指定类型的【建筑数量】
    public int CountOff(ElementType type)
    {
        int num = 0;
        foreach (Tile t in AllTiles)
        {
            if (t.TileType == type)
            {
                num++;
            }

            //顺便遍历所有按钮 如果有处于禁用状态的按钮 将其恢复
            t.GetComponent<Button>().interactable = true;
        }
        return num;
    }

    //敌方支援
    private void Assisted()
    {
        foreach (Tile t in AllTiles)
        {
            if (t.TileType == ElementType.Enemy)
            {
                t.TileLevel *= 2;
                t.UpdateTile();
            }
        }
    }

    //更新空方块列表与【建筑行为】
    private void UpdateEmptyTiles()
    {
        //清空空方快列表
        EmptyTiles.Clear();
        //遍历所有方块
        foreach (Tile t in AllTiles)
        {
            /// 7.建筑行为与空方快列表的更新
            switch (t.TileType)
            {
                case ElementType.Empty:
                    //更新空方块列表
                    EmptyTiles.Add(t);
                    break;

                #region 产出资源
                case ElementType.Power:
                    //仅在玩家发生过移动时产生资源
                    if (move)
                    {
                        Power.Instance.Numerical += 10; //获得电力 （根据建筑等级？
                    }
                    break;
                case ElementType.Mall:
                    if (move)
                    {
                        Money.Instance.Numerical += 5; //获得金钱 （根据建筑等级？
                    }
                    break;
                #endregion

                #region 我方建筑行为
                //攻击塔
                case ElementType.Tower:
                    if (TowerAttack(t.indCol, t.indRow, ElementType.Enemy)) ;               //近战型敌人
                    else if (TowerAttack(t.indCol, t.indRow, ElementType.TowerEnemy)) ;     //远程型敌人
                    else if (TowerAttack(t.indCol, t.indRow, ElementType.BuilderEnemy)) ;   //造墙型敌人
                    else if (TowerAttack(t.indCol, t.indRow, ElementType.AssistedEnemy)) ;  //支援型敌人
                    break;

                //干扰磁场
                case ElementType.Magnetic:
                    MagneticEvent(t.indCol, t.indRow);  //反推可移动单位
                    t.TileType = ElementType.Empty;     //销毁自身
                    break;
                #endregion

                #region 敌方行为
                //敌方远程兵
                case ElementType.TowerEnemy:
                    if (TowerAttack(t.indCol, t.indRow, ElementType.Player))            //主角
                    {
                        PanelManager.Instance.GameOver("<size=20> プレーヤーが遠隔に攻撃された </size>\n", " 遠隔型敵に遠避けるほうがいいよ");
                    }
                    else if (TowerAttack(t.indCol, t.indRow, ElementType.Access))        //避难所内部
                    {
                        PanelManager.Instance.GameOver("<size=20> プレーヤーが遠隔に攻撃された </size>\n", " 遠隔型敵に遠避けるほうがいいよ");
                    }
                    else if (TowerAttack(t.indCol, t.indRow, ElementType.Refuge)) ;     //避难所
                    else if (TowerAttack(t.indCol, t.indRow, ElementType.Power)) ;      //发电站
                    else if (TowerAttack(t.indCol, t.indRow, ElementType.Mall)) ;       //商场
                    else if (TowerAttack(t.indCol, t.indRow, ElementType.Tower)) ;      //攻击塔
                    else if (TowerAttack(t.indCol, t.indRow, ElementType.Material)) ;   //建材
                    break;

                case ElementType.AssistedEnemy:
                    //支援型敌人 使全场的敌人等级翻倍
                    Assisted();
                    break;
                    #endregion
            }
        }
    }

    //【远程攻击】 参数是塔的x,y坐标，和要攻击的单位类型
    private bool TowerAttack(int x, int y, ElementType type)
    {
        bool attack = false;    //【T】本回合已进行过攻击 【F】未进行过攻击

        //横向 先左后右 (x-3,y) -> (x+3,y)
        for (int j = x - 3; j <= x + 3; j++)
        {
            #region 攻击前的判断
            if (attack)
            {
                Debug.Log("已进行过攻击");
                break;
            }
            if (j < 0 || j > 7)
            {
                continue;
            }
            #endregion

            #region 发动攻击
            if (AllTiles[y, j].TileType == type)
            {
                int consume = AllTiles[y, j].TileLevel * 2;
                int surplus = Power.Instance.Numerical - consume;
                if (surplus > 20)
                {
                    AllTiles[y, j].TileLevel = 0;
                    AllTiles[y, j].TileType = ElementType.Empty;

                    attack = true;

                    Power.Instance.Numerical = surplus;
                    string str = "攻击消耗" + consume;
                    Debug.Log(str);

                    return true;
                }
            }
            #endregion
        }

        //纵向 先下后上 (x,y-3) -> (x,y+3)
        for (int i = y - 3; i <= y + 3; i++)
        {
            #region 攻击前的判断
            //每回合只攻击一个单位
            if (attack)
            {
                Debug.Log("已进行过攻击");
                break;
            }
            //防止数组下标越界
            if (i < 0 || i > 10)
            {
                continue;
            }
            #endregion

            #region 发动攻击
            //定位到攻击范围内的格子 判断其是否为攻击目标
            if (AllTiles[i, x].TileType == type)
            {
                int consume = AllTiles[i, x].TileLevel * 2;    //攻击消耗
                int surplus = Power.Instance.Numerical - consume;   //攻击后剩余电力
                                                                    //判断是否有足够的资源进行攻击
                if (surplus > 20)
                {
                    //清空敌人的等级和样式
                    AllTiles[i, x].TileLevel = 0;
                    AllTiles[i, x].TileType = ElementType.Empty;

                    attack = true;      //关闭攻击开关
                                        //消耗相应资源
                    Power.Instance.Numerical = surplus;
                    string str = "攻击消耗" + consume;
                    Debug.Log(str);

                    return true;
                }
            }
            #endregion
        }

        return false;
    }

    //磁场事件 目前定义在回合结束时触发
    public void MagneticEvent(int x, int y)
    {
        //左 x = -3 -2 -1
        for (int i = x - 3; i < x; i++)
        {
            //防止数组下标越界
            if (i - 1 < 0)
            {
                continue;
            }

            //如果是个可移动的单位 且其身后为空或是可移动单位
            if (AllTiles[y, i].TileType < ElementType.Tower && AllTiles[y, i].TileType != ElementType.Empty
                && AllTiles[y, i - 1].TileType == ElementType.Empty)
            {
                //传值给后一个格子
                AllTiles[y, i - 1].TileLevel = AllTiles[y, i].TileLevel;
                AllTiles[y, i - 1].TileType = AllTiles[y, i].TileType;

                //清空自身
                AllTiles[y, i].TileLevel = 0;
                AllTiles[y, i].TileType = ElementType.Empty;
            }
        }

        //右 x = 3 2 1
        for (int i = x + 3; i > x; i--)
        {
            if (i + 1 > 7)
            {
                continue;
            }

            if (AllTiles[y, i].TileType < ElementType.Tower && AllTiles[y, i].TileType != ElementType.Empty
                && AllTiles[y, i + 1].TileType == ElementType.Empty)
            {
                AllTiles[y, i + 1].TileLevel = AllTiles[y, i].TileLevel;
                AllTiles[y, i + 1].TileType = AllTiles[y, i].TileType;

                AllTiles[y, i].TileLevel = 0;
                AllTiles[y, i].TileType = ElementType.Empty;
            }
        }

        //上 y = 3 2 1
        for (int i = y + 3; i > y; i--)
        {
            //防止数组下标越界
            if (i + 1 > 10)
            {
                continue;
            }

            if (AllTiles[i, x].TileType < ElementType.Tower && AllTiles[i, x].TileType != ElementType.Empty
                && AllTiles[i + 1, x].TileType == ElementType.Empty)
            {
                AllTiles[i + 1, x].TileLevel = AllTiles[i, x].TileLevel;
                AllTiles[i + 1, x].TileType = AllTiles[i, x].TileType;

                AllTiles[i, x].TileLevel = 0;
                AllTiles[i, x].TileType = ElementType.Empty;
            }
        }

        //下y = -3 -2 -1
        for (int i = y - 3; i < y; i++)
        {
            if (i - 1 < 0)
            {
                continue;
            }

            if (AllTiles[i, x].TileType < ElementType.Tower && AllTiles[i, x].TileType != ElementType.Empty
                && AllTiles[i - 1, x].TileType == ElementType.Empty)
            {
                AllTiles[i - 1, x].TileLevel = AllTiles[i, x].TileLevel;
                AllTiles[i - 1, x].TileType = AllTiles[i, x].TileType;

                AllTiles[i, x].TileLevel = 0;
                AllTiles[i, x].TileType = ElementType.Empty;
            }
        }
    }

    #region 游戏胜负的判定
    //胜利 检查场上是否还存在敌人
    bool NotEnemy()
    {
        foreach (Tile t in AllTiles)
        {
            //如果还存在敌人
            if (t.TileType == ElementType.Enemy)
            {
                return false;
            }
        }
        //不存在敌人
        return true;
    }

    //失败 检查是否还有可移动的方块【已弃用】
    bool CanMove()
    {
        //无视剩余空方快大于零的情况
        if (EmptyTiles.Count > 0)
            return true;
        //无视方块周围还有相同数字方块的情况
        else
        {
            //check columns
            //for (int i = 0; i < colums.Count; i++)
            //    for (int j = 0; j < rows.Count - 1; j++)
            //        if (AllTiles[j, i].Number == AllTiles[j + 1, i].Number)
            //            return true;

            ////check rows
            //for (int i = 0; i < rows.Count; i++)
            //    for (int j = 0; j < colums.Count - 1; j++)
            //        if (AllTiles[i, j].Number == AllTiles[i, j + 1].Number)
            //            return true;
        }

        //既没有空方快，也没有可以合并的方块时，游戏失败
        return false;
    }
    #endregion

    //debug用
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PanelManager.Instance.YouWon();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            PanelManager.Instance.GameOver("手动结束", "");
        }
        //清空所有可移动的单位
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            foreach (Tile t in AllTiles)
            {
                if (t.TileType == ElementType.Material || t.TileType == ElementType.Enemy)
                {
                    t.TileType = ElementType.Empty;
                }
            }
        }
        //清空主角外的所有单位
        else if (Input.GetKeyDown(KeyCode.X))
        {
            foreach (Tile t in AllTiles)
            {
                if (t.TileType != ElementType.Player && t.TileType != ElementType.Access)
                {
                    t.TileType = ElementType.Empty;
                }
            }
        }
        //制造一个敌人
        else if (Input.GetKeyDown(KeyCode.A))
        {
            Generate(ElementType.AssistedEnemy);

        }
        //获得大量资源
        else if (Input.GetKeyDown(KeyCode.L))
        {
            Material.Instance.Numerical += 3000;  //获得建材
            Power.Instance.Numerical += 3000;     //获得电力
            Money.Instance.Numerical += 3000;     //获得金钱
        }
    }

}
