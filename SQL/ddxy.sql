USE ddxy;

SET NAMES utf8mb4;

DROP TABLE IF EXISTS `user`;
CREATE TABLE `user`
(
    `id`            INT UNSIGNED     NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `username`      VARCHAR(32)      NOT NULL
        COMMENT '用户名',
    `password`      VARCHAR(64)      NOT NULL
        COMMENT '用户密码',
    `passSalt`      VARCHAR(64)      NOT NULL
        COMMENT '密码盐',
    `status`        TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '账号状态, 0-正常, 1-冻结',
    `type`          TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '账号类型, 0-正常, 1-GM, 2-Robot',
    `parentId`      INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '所属代理id, 0表示平台用户',
    `platform`      VARCHAR(128)     NOT NULL DEFAULT ''
        COMMENT '客户端类型',
    `simulation`    BOOLEAN          NOT NULL DEFAULT 0
        COMMENT '是否模拟器',
    `deviceId`      VARCHAR(256)     NOT NULL DEFAULT ''
        COMMENT '设备机器码',
    `createTime`    INT UNSIGNED     NOT NULL
        COMMENT '创建时间',
    `lastLoginIp`   VARCHAR(200)     NOT NULL DEFAULT ''
        COMMENT '上次登录IP',
    `lastLoginTime` INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '上次登录时间',
    `lastUseRoleId` INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '上次使用的角色id',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_username` (`username`),
    INDEX `idx_parentId` (`parentId`),
    INDEX `idx_createTime` (`createTime`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    AUTO_INCREMENT = 100000
    COMMENT ='用户表';


DROP TABLE IF EXISTS `role`;
CREATE TABLE `role`
(
    `id`              INT UNSIGNED     NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `uid`             INT UNSIGNED     NOT NULL
        COMMENT '用户id',
    `sid`             INT UNSIGNED     NOT NULL
        COMMENT '区服id',
    `status`          TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '角色状态',
    `type`            TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '角色类型, 0-正常, 1-GM, 2-Robot',
    `nickname`        VARCHAR(32)      NOT NULL
        COMMENT '昵称',
    `cfgId`           INT UNSIGNED     NOT NULL
        COMMENT '配置id',
    `sex`             TINYINT UNSIGNED NOT NULL DEFAULT 1
        COMMENT '性别, 1-男，2-女',
    `race`            TINYINT UNSIGNED NOT NULL DEFAULT 1
        COMMENT '种族, 1-人，2-仙，3-魔，4-鬼',
    `relive`          TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '转生等级',
    `level`           TINYINT UNSIGNED NOT NULL DEFAULT 1
        COMMENT '等级',
    `exp`             BIGINT UNSIGNED  NOT NULL DEFAULT 0
        COMMENT '经验值',
    `silver`          INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '银元值',
    `jade`            INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '仙玉值',
    `bindJade`        INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '绑定仙玉',
    `contrib`         INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '贡献值',
    `sldhGongJi`      INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '水路大会功绩',
    `guoShi`          INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '郭氏积分',
    `skins`           VARCHAR(1000)    NOT NULL DEFAULT '{"has":[],"use":[]}'
        COMMENT '拥有的皮肤（暂时只含足迹和特效）',
    `bianshen`        VARCHAR(1000)    NOT NULL DEFAULT '{"cards":{},"wuxing":{},"current":{"id":0,"timestamp":"0"}}'
        COMMENT '变身卡及五行修炼',
    `xingzhen`        VARCHAR ( 1000 ) NOT NULL DEFAULT '{"unlocked":{},"used":0}'
        COMMENT '星阵',
    `child`           VARCHAR ( 1000 ) NOT NULL DEFAULT ''
        COMMENT '孩子信息',
    `expExchangeTimes` INT UNSIGNED    NOT NULL DEFAULT 0
        COMMENT '经验兑换属性点次数',
    `mapId`           INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '当前所在地图id',
    `mapX`            INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '地图坐标X',
    `mapY`            INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '地图坐标Y',
    `skills`          VARCHAR(200)     NOT NULL DEFAULT ''
        COMMENT '6个技能的熟练度用,分割',
    `color1`          INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '染色1',
    `color2`          INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '染色2',
    `sectId`          INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '当前加入的帮派id',
    `sectContrib`     INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '当前帮派贡献值',
    `sectJob`         TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '在帮派中的职位',
    `sectJoinTime`    INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '入帮时间',
    `xlLevel`         INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '修炼等级',
    `star`            INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '击杀地煞星 星级',
    `shane`           INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '监禁的结束时间',
    `relives`         VARCHAR(30)      NOT NULL DEFAULT ''
        COMMENT '转生信息race_sex',
    `rewards`         VARCHAR(200)     NOT NULL DEFAULT ''
        COMMENT '等级奖励',
    `sldh`            VARCHAR(100)     NOT NULL DEFAULT ''
        COMMENT '水陆大会',
    `singlePk`        VARCHAR(100)     NOT NULL DEFAULT ''
        COMMENT '单人PK',
    `flags`           INT              NOT NULL DEFAULT 0
        COMMENT '各种开关',
    `autoSkill`       INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '自动技能',
    `autoSyncSkill`   BIT              NOT NULL DEFAULT TRUE
        COMMENT '是否自动同步',
    `totalPay`        INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '累计充值额',
    `totalPayRewards` VARCHAR(200)     NOT NULL DEFAULT ''
        COMMENT '累计充值领取金额集合',
    `totalPayBS`      INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '累计充值额',
    `dailyPay`        INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '今日充值额',
    `dailyPayTime`    INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '今日充值额记录的日期',
    `dailyPayRewards` VARCHAR(200)     NOT NULL DEFAULT ''
        COMMENT '今日充值领取金额集合',
    `safeCode`        VARCHAR(32)      NOT NULL DEFAULT ''
        COMMENT '安全锁',
    `safeLocked`      BIT              NOT NULL DEFAULT FALSE
        COMMENT '当前是否已上锁',
    `spread`          INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '绑定的推广人',
    `spreadTime`      INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '绑定时间',
    `parentId`        INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '所属代理id, 0表示平台用户',
    `online`          BIT              NOT NULL DEFAULT FALSE
        COMMENT '当前是否在线',
    `onlineTime`      INT UNSIGNED     NOT NULL
        COMMENT '上次上线时间',
    `createTime`      INT UNSIGNED     NOT NULL
        COMMENT '创建时间',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_nickname` (`nickname`),
    INDEX `idx_uid` (`uid`),
    INDEX `idx_sid` (`sid`),
    INDEX `idx_sectId` (`sectId`),
    INDEX `idx_spread` (`spread`),
    INDEX `idx_parentId` (`parentId`),
    INDEX `idx_createTime` (`createTime`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    AUTO_INCREMENT = 100000
    COMMENT ='角色表';


DROP TABLE IF EXISTS `role_ext`;
CREATE TABLE `role_ext`
(
    `rid`   INT UNSIGNED  NOT NULL
        COMMENT 'rid',
    `items` VARCHAR(5000) NOT NULL DEFAULT ''
        COMMENT '背包中item和数量',
    `repos` VARCHAR(5000) NOT NULL DEFAULT ''
        COMMENT '仓库中item和数量',
    `mails` VARCHAR(1000) NOT NULL DEFAULT ''
        COMMENT '我处理过的全服邮件id及其操作',
    `tiance` LONGTEXT COMMENT '天策符数据',
    `qiegeLevel` INT UNSIGNED    NOT NULL DEFAULT 0
        COMMENT '切割等级',
    `qiegeExp` INT UNSIGNED    NOT NULL DEFAULT 0
        COMMENT '切割经验',
    `done1stDraw` boolean NOT NULL DEFAULT 0
        COMMENT '是否已经完成新手抽奖？',
    PRIMARY KEY (`rid`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='角色扩展表';


DROP TABLE IF EXISTS `title`;
CREATE TABLE `title`
(
    `id`         INT UNSIGNED NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `rid`        INT UNSIGNED NOT NULL
        COMMENT '所属角色id',
    `cfgId`      INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '称号模板id',
    `text`       VARCHAR(100) NOT NULL DEFAULT ''
        COMMENT '称号文本',
    `active`     BIT          NOT NULL DEFAULT FALSE
        COMMENT '是否穿戴',
    `createTime` INT UNSIGNED NOT NULL
        COMMENT '创建时间',
    `expireTime` INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '过期时间',
    PRIMARY KEY (`id`),
    INDEX `idx_rid` (`rid`),
    INDEX `idx_cfgId` (`cfgId`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='称号表';


DROP TABLE IF EXISTS `equip`;
CREATE TABLE `equip`
(
    `id`          INT UNSIGNED     NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `rid`         INT UNSIGNED     NOT NULL
        COMMENT '所属角色id',
    `category`    TINYINT UNSIGNED NOT NULL
        COMMENT '装备类型',
    `cfgId`       INT UNSIGNED     NOT NULL
        COMMENT '配置id',
    `starCount`   INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '升星数',
    `starExp`     INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '升星经验',
    `gem`         TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '宝石镶嵌数量',
    `grade`       TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '品阶',
    `place`       TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '装备存放位置, 0-未获得, 1-穿戴, 2-背包, 3-仓库',
    `baseAttrs`   VARCHAR(2000)    NOT NULL DEFAULT ''
        COMMENT '基础属性',
    `refineAttrs` VARCHAR(2000)    NOT NULL DEFAULT ''
        COMMENT '炼化属性',
    `needAttrs`   VARCHAR(2000)    NOT NULL DEFAULT ''
        COMMENT '需求属性',
    `refine`      VARCHAR(2000)    NOT NULL DEFAULT ''
        COMMENT '炼化预览数据',
    `refineList`  VARCHAR(2000)    NOT NULL DEFAULT ''
        COMMENT '炼化预览数据（多次炼化）',
    `recast`      VARCHAR(2000)    NOT NULL DEFAULT ''
        COMMENT '重铸预览数据',
    `createTime`  INT UNSIGNED     NOT NULL
        COMMENT '创建时间',
    PRIMARY KEY (`id`),
    INDEX `idx_rid` (`rid`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='装备表';


DROP TABLE IF EXISTS `ornament`;
CREATE TABLE `ornament`
(
    `id`         INT UNSIGNED     NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `rid`        INT UNSIGNED     NOT NULL
        COMMENT '所属角色id',
    `cfgId`      INT UNSIGNED     NOT NULL
        COMMENT '配置id',
    `grade`      TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '品阶',
    `place`      TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '装备存放位置, 0-未获得, 1-穿戴, 2-背包, 3-仓库',
    `baseAttrs`  VARCHAR(2000)    NOT NULL DEFAULT ''
        COMMENT '基础属性',
    `recast`     VARCHAR(2000)    NOT NULL DEFAULT ''
        COMMENT '重铸预览数据',
    `createTime` INT UNSIGNED     NOT NULL
        COMMENT '创建时间',
    PRIMARY KEY (`id`),
    INDEX `idx_rid` (`rid`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='配饰表';


DROP TABLE IF EXISTS `task`;
CREATE TABLE `task`
(
    `id`          INT UNSIGNED NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `rid`         INT UNSIGNED NOT NULL
        COMMENT '所属角色id',
    `complets`    VARCHAR(100) NOT NULL DEFAULT ''
        COMMENT '已完成的剧情任务id集合',
    `states`      VARCHAR(500) NOT NULL DEFAULT ''
        COMMENT '已接受的任务id及当前的step',
    `dailyStart`  VARCHAR(500) NOT NULL DEFAULT ''
        COMMENT '已经接受的日常任务group集合',
    `dailyCnt`    VARCHAR(500) NOT NULL DEFAULT ''
        COMMENT '日常任务计数',
    `instanceCnt` VARCHAR(500) NOT NULL DEFAULT ''
        COMMENT '副本任务计数',
    `activeScore` VARCHAR(500) NOT NULL DEFAULT ''
        COMMENT '活动积分',
    `beenTake`    VARCHAR(500) NOT NULL DEFAULT ''
        COMMENT '今日奖励领取',
    `starNum`     INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '今日杀星次数',
    `monkeyNum`   INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '今日灵猴次数',
    `jinChanSongNum`   INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '今日金蟾送宝次数',
    `eagleNum` INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '今日金翅大鹏次数',
    `updateTime`  INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '上次刷新时间',
    `createTime`  INT UNSIGNED NOT NULL
        COMMENT '创建时间',
    PRIMARY KEY (`id`),
    INDEX `idx_rid` (`rid`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='角色-任务表';


DROP TABLE IF EXISTS `pet`;
CREATE TABLE `pet`
(
    `id`            INT UNSIGNED     NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `rid`           INT UNSIGNED     NOT NULL
        COMMENT '所属角色id',
    `cfgId`         INT UNSIGNED     NOT NULL
        COMMENT '配置id',
    `name`          VARCHAR(12)      NOT NULL
        COMMENT '宠物名称',
    `relive`        TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '转生等级',
    `level`         TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '等级',
    `exp`           BIGINT UNSIGNED  NOT NULL DEFAULT 0
        COMMENT '经验',
    `intimacy`      INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '亲密度',
    `hp`            INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '基础气血',
    `mp`            INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '基础法力',
    `atk`           INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '基础攻击',
    `spd`           INT              NOT NULL DEFAULT 0
        COMMENT '基础速度',
    `rate`          INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '成长率',
    `quality`       TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '洗练品级',
    `keel`          INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '龙骨',
    `unlock`        INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '使用了多少聚魂丹',
    `skills`        VARCHAR(500)     NOT NULL DEFAULT ''
        COMMENT '学习到的技能id',
    `ssSkill`       INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '神兽技能',
    `jxLevel`       INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '觉醒等级',
    `apAttrs`       VARCHAR(100)     NOT NULL DEFAULT ''
        COMMENT '加点值',
    `elements`      VARCHAR(100)     NOT NULL DEFAULT ''
        COMMENT '五行元素',
    `refineLevel`   INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '修炼等级',
    `refineExp`     INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '修炼经验',
    `refineAttrs`   VARCHAR(2000)    NOT NULL DEFAULT ''
        COMMENT '修炼属性',
    `fly`           INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '%10表示飞升次数, /10表示飞升增加的属性 1hp 2mp 3atk 4spd',
    `color`         INT              NOT NULL DEFAULT 0
        COMMENT '变色(-1:变色未成功，0:未变色, >0变色结果)',
    `sxOrder`       INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '闪现支援顺序',
    `autoSkill`     INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '自动技能',
    `autoSyncSkill` BIT              NOT NULL DEFAULT FALSE
        COMMENT '是否自动同步',
    `washData`      VARCHAR(500)     NOT NULL DEFAULT ''
        COMMENT '未替换的洗练数据',
    `active`        BIT              NOT NULL DEFAULT FALSE
        COMMENT '是否参战',
    `talks` LONGTEXT COMMENT '闲聊数据',
    `createTime`    INT UNSIGNED     NOT NULL
        COMMENT '创建时间',
    PRIMARY KEY (`id`),
    INDEX `idx_rid` (`rid`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='宠物表';


DROP TABLE IF EXISTS `mount`;
CREATE TABLE `mount`
(
    `id`         INT UNSIGNED     NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `rid`        INT UNSIGNED     NOT NULL
        COMMENT '所属角色id',
    `cfgId`      INT UNSIGNED     NOT NULL
        COMMENT '配置id',
    `name`       VARCHAR(12)      NOT NULL
        COMMENT '坐骑名称',
    `level`      TINYINT UNSIGNED NOT NULL DEFAULT 1
        COMMENT '等级',
    `exp`        BIGINT UNSIGNED  NOT NULL DEFAULT 0
        COMMENT '经验',
    `hp`         INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '体力',
    `spd`        INT              NOT NULL DEFAULT 0
        COMMENT '基础速度',
    `rate`       INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '成长率',
    `skills`     VARCHAR(200)     NOT NULL DEFAULT ''
        COMMENT '技能id和熟练度',
    `pets`       VARCHAR(1000)    NOT NULL DEFAULT ''
        COMMENT '已管制的宠物id集合',
    `washData`   VARCHAR(500)     NOT NULL DEFAULT ''
        COMMENT '未替换的洗练数据',
    `active`     BIT              NOT NULL DEFAULT FALSE
        COMMENT '是否乘骑',
    `locked`     BIT              NOT NULL DEFAULT FALSE
        COMMENT '是否锁定',
    `createTime` INT UNSIGNED     NOT NULL
        COMMENT '创建时间',
    PRIMARY KEY (`id`),
    INDEX `idx_rid` (`rid`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='坐骑表';


DROP TABLE IF EXISTS `partner`;
CREATE TABLE `partner`
(
    `id`         INT UNSIGNED     NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `rid`        INT UNSIGNED     NOT NULL
        COMMENT '所属者角色id',
    `cfgId`      INT UNSIGNED     NOT NULL
        COMMENT '配置id',
    `relive`     TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '转生等级',
    `level`      TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '等级',
    `exp`        BIGINT UNSIGNED  NOT NULL DEFAULT 0
        COMMENT '经验值',
    `pos`        TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '是否参战',
    `createTime` INT UNSIGNED     NOT NULL
        COMMENT '创建时间',
    PRIMARY KEY (`id`),
    INDEX `idx_rid` (`rid`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='角色伙伴';


DROP TABLE IF EXISTS `scheme`;
CREATE TABLE `scheme`
(
    `id`         INT UNSIGNED  NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `rid`        INT UNSIGNED  NOT NULL
        COMMENT '所属者角色id',
    `name`       VARCHAR(8)
        COMMENT '方案名字',
    `equips`     VARCHAR(100)  NOT NULL DEFAULT ''
        COMMENT '装备',
    `ornaments`  VARCHAR(100)  NOT NULL DEFAULT ''
        COMMENT '配饰',
    `apAttrs`    VARCHAR(100)  NOT NULL DEFAULT ''
        COMMENT '加点属性',
    `xlAttrs`    VARCHAR(2000) NOT NULL DEFAULT ''
        COMMENT '修炼属性',
    `relives`    VARCHAR(30)   NOT NULL DEFAULT ''
        COMMENT '转生信息race_sex, 使用回梦丹修改的',
    `active`     BIT           NOT NULL DEFAULT FALSE
        COMMENT '是否激活',
    `createTime` INT UNSIGNED  NOT NULL
        COMMENT '创建时间',
    PRIMARY KEY (`id`),
    INDEX `idx_rid` (`rid`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='角色属性方案';


DROP TABLE IF EXISTS `sect`;
CREATE TABLE `sect`
(
    `id`         INT UNSIGNED NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `sid`        INT UNSIGNED NOT NULL
        COMMENT '区服id',
    `name`       VARCHAR(32)  NOT NULL
        COMMENT '帮派名字',
    `desc`       VARCHAR(200) NOT NULL DEFAULT ''
        COMMENT '帮派宗旨',
    `ownerId`    INT UNSIGNED NOT NULL
        COMMENT '帮主角色id',
    `memberNum`  INT UNSIGNED NOT NULL DEFAULT 1
        COMMENT '人数',
    `contrib`    INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '帮派总贡献',
    `createTime` INT UNSIGNED NOT NULL
        COMMENT '创建时间',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_name` (`name`),
    INDEX `idx_sid` (`sid`),
    INDEX `idx_rid` (`ownerId`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    AUTO_INCREMENT = 10000
    COMMENT ='帮派';


DROP TABLE IF EXISTS `mall`;
CREATE TABLE `mall`
(
    `id`         INT UNSIGNED     NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `sid`        INT UNSIGNED     NOT NULL
        COMMENT '区服id',
    `rid`        INT UNSIGNED     NOT NULL
        COMMENT '卖方角色id',
    `dbId`       INT UNSIGNED     NOT NULL
        COMMENT '商品实例id',
    `cfgId`      INT UNSIGNED     NOT NULL
        COMMENT '商品配置id',
    `num`        INT UNSIGNED     NOT NULL
        COMMENT '剩余数量',
    `sellNum`    INT UNSIGNED     NOT NULL
        COMMENT '已出售的数量',
    `price`      INT UNSIGNED     NOT NULL
        COMMENT '物品单价',
    `type`       TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '物品类型,参考MallItemType',
    `kind`       TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '物品分类,参考MallItemKind',
    `detail`     VARBINARY(1000)  NULL
        COMMENT '物品详情,主要装备和宠物详情',
    `createTime` INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '上架时间',
    PRIMARY KEY (`id`),
    INDEX `idx_sid` (`sid`),
    INDEX `idx_rid` (`rid`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='摆摊商品';


DROP TABLE IF EXISTS `mail`;
CREATE TABLE `mail`
(
    `id`         INT UNSIGNED     NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `sid`        INT UNSIGNED     NOT NULL
        COMMENT '区服id',
    `sender`     INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '发送者角色id, 0表示系统',
    `recver`     INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '接收者角色id, 0表示全区角色',
    `admin`      INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '后台账号id',
    `type`       TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '邮件类型',
    `text`       VARCHAR(1000)    NOT NULL DEFAULT ''
        COMMENT '邮件内容',
    `items`      VARCHAR(1000)    NOT NULL DEFAULT ''
        COMMENT '邮件携带内容',
    `minRelive`  TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '最低转生等级',
    `minLevel`   INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '最低等级',
    `maxRelive`  TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '最高转生等级',
    `maxLevel`   INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '最高等级',
    `remark`     VARCHAR(500)     NOT NULL DEFAULT ''
        COMMENT '备注',
    `createTime` INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '发布时间',
    `pickedTime` INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '领取时间',
    `deleteTime` INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '删除时间',
    `expireTime` INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '过期时间',
    PRIMARY KEY (`id`),
    INDEX `idx_sid` (`sid`),
    INDEX `idx_sender` (`sender`),
    INDEX `idx_recver` (`recver`),
    INDEX `idx_admin` (`admin`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='邮件';


DROP TABLE IF EXISTS `sldh`;
CREATE TABLE `sldh`
(
    `id`       INT UNSIGNED NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `sid`      INT UNSIGNED NOT NULL
        COMMENT '区服id',
    `season`   INT UNSIGNED NOT NULL DEFAULT 1
        COMMENT '当前第几季',
    `turn`     INT UNSIGNED NOT NULL DEFAULT 1
        COMMENT '当前第几轮',
    `lastTime` INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '上次开始的时间',
    `slzs`     VARCHAR(500) NOT NULL DEFAULT ''
        COMMENT '上季获得水路战神称号的角色id集合',
    PRIMARY KEY (`id`),
    INDEX `idx_sid` (`sid`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    AUTO_INCREMENT = 1000
    COMMENT ='水陆大会';

DROP TABLE IF EXISTS `ssjl`;
CREATE TABLE `ssjl`
(
    `id`       INT UNSIGNED NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `sid`      INT UNSIGNED NOT NULL
        COMMENT '区服id',
    `season`   INT UNSIGNED NOT NULL DEFAULT 1
        COMMENT '当前第几季',
    `lastTime` INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '上次开始的时间',
    `reward`   VARCHAR(500) NOT NULL DEFAULT ''
        COMMENT '上季获得的神兽及角色ID',
    PRIMARY KEY (`id`),
    INDEX `idx_sid` (`sid`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    AUTO_INCREMENT = 1000
    COMMENT ='神兽降临';

DROP TABLE IF EXISTS `sectWar`;
CREATE TABLE `sectWar`
(
    `id`       INT UNSIGNED NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `sid`      INT UNSIGNED NOT NULL
        COMMENT '区服id',
    `season`   INT UNSIGNED NOT NULL DEFAULT 1
        COMMENT '当前第几季',
    `turn`     INT UNSIGNED NOT NULL DEFAULT 1
        COMMENT '当前第几轮',
    `lastTime` INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '上次开始的时间',
    PRIMARY KEY (`id`),
    INDEX `idx_sid` (`sid`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    AUTO_INCREMENT = 1000
    COMMENT ='帮战';


DROP TABLE IF EXISTS `singlePk`;
CREATE TABLE `singlePk`
(
    `id`       INT UNSIGNED NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `sid`      INT UNSIGNED NOT NULL
        COMMENT '区服id',
    `season`   INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '当前第几季',
    `pkzs`     VARCHAR(200) NOT NULL DEFAULT ''
        COMMENT '上季获得PK战神称号的角色id集合',
    `lastTime` INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '上次开始的时间',
    PRIMARY KEY (`id`),
    INDEX `idx_sid` (`sid`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    AUTO_INCREMENT = 1000
    COMMENT ='单人PK';


DROP TABLE IF EXISTS `pay`;
CREATE TABLE `pay`
(
    `id`         INT UNSIGNED     NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `rid`        INT UNSIGNED     NOT NULL
        COMMENT '角色id',
    `money`      INT UNSIGNED     NOT NULL
        COMMENT '下单金额,单位元',
    `jade`       INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '支付后可以获得的仙玉数量',
    `bindJade`   INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '支付后可以获得的积分数量',
    `payChannel` TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '支付渠道',
    `payType`    TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '支付方式',
    `remark`     VARCHAR(200)     NOT NULL DEFAULT ''
        COMMENT '订单备注',
    `order`      VARCHAR(100)     NOT NULL DEFAULT ''
        COMMENT '支付渠道订单编号',
    `status`     TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '0-已创建,1-支付成功, 2-支付失败',
    `createTime` INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '创建订单时间',
    `updateTime` INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '支付结果时间',
    `delivTime`  INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '发货时间',
    PRIMARY KEY (`id`),
    INDEX `idx_rid` (`rid`),
    INDEX `idx_payChannel` (`payChannel`),
    INDEX `payType` (`payType`),
    INDEX `order` (`order`),
    INDEX `idx_status` (`status`),
    INDEX `idx_createTime` (`createTime`),
    INDEX `idx_updateTime` (`updateTime`),
    INDEX `idx_delivTime` (`delivTime`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='线上支付表';


DROP TABLE IF EXISTS `recharge_role`;
CREATE TABLE `recharge_role`
(
    `id`          INT UNSIGNED NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `roleId`      INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '角色id',
    `parentId`    INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '角色所属代理id',
    `opId`        INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '操作员id',
    `opName`      VARCHAR(32)  NOT NULL DEFAULT ''
        COMMENT '操作员昵称',
    `opInvitCode` VARCHAR(12)  NOT NULL DEFAULT ''
        COMMENT '操作员邀请码',
    `money`       INT          NOT NULL DEFAULT 0
        COMMENT '充值额度',
    `remark`      VARCHAR(500) NOT NULL DEFAULT ''
        COMMENT '备注',
    `createTime`  INT UNSIGNED NOT NULL
        COMMENT '创建时间',
    PRIMARY KEY (`id`),
    INDEX `idx_opId` (`opId`),
    INDEX `idx_roleId` (`roleId`),
    INDEX `idx_parentId` (`parentId`),
    INDEX `idx_createTime` (`createTime`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='角色线下充值表';

DROP TABLE IF EXISTS `chat_msg`;
CREATE TABLE `chat_msg`
(
    `id`         INT UNSIGNED NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `fromRid`    INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '发送角色id',
    `toRid`      INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '接收角色id',
    `msgType`    TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '消息类型',
    `msg`       VARCHAR(1024) NOT NULL DEFAULT ''
        COMMENT '消息文本',
    `sendTime` INT UNSIGNED NOT NULL
        COMMENT '发送时间',
    PRIMARY KEY (`id`),
    INDEX `idx_frid` (`fromRid`),
    INDEX `idx_trid` (`toRid`),
    INDEX `idx_msgtype` (`msgType`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='聊天记录表';

DROP TABLE IF EXISTS `red_send_record`;
CREATE TABLE `red_send_record`
(
    `id`         INT UNSIGNED NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `sid`        INT UNSIGNED NOT NULL
        COMMENT '区服id',
    `roleId`     INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '发送角色ID',
    `redType`    TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '红包类型',
    `sectId`     INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '帮派ID（帮派红包有效）',
    `jade`       INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '仙玉',
    `total`      INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '总计个数',
    `wish`       VARCHAR(512) NOT NULL DEFAULT ''
        COMMENT '祝福',
    `left`       INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '剩余个数',
    `reciver`    LONGTEXT
        COMMENT '接收者列表',
    `sendTime`   INT UNSIGNED NOT NULL
        COMMENT '发送时间',
    PRIMARY KEY (`id`),
    INDEX `idx_roleId` (`roleId`),
    INDEX `idx_redType` (`redType`),
    INDEX `idx_sectId` (`sectId`),
    INDEX `idx_left` (`left`),
    INDEX `idx_sendTime` (`sendTime`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    AUTO_INCREMENT = 1000
    COMMENT ='红包发送记录表';

DROP TABLE IF EXISTS `red_recive_record`;
CREATE TABLE `red_recive_record`
(
    `id`         INT UNSIGNED NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `sid`        INT UNSIGNED NOT NULL
        COMMENT '区服id',
    `reciveId`     INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '接收角色ID',
    `sendId`     INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '发送角色ID',
    `redId`      INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '红包ID',
    `redType`    TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '红包类型',
    `jade`       INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '仙玉',
    `reciveTime` INT UNSIGNED NOT NULL
        COMMENT '接收时间',
    PRIMARY KEY (`id`),
    INDEX `idx_sid` (`sid`),
    INDEX `idx_reciveId` (`reciveId`),
    INDEX `idx_sendId` (`sendId`),
    INDEX `idx_redId` (`redId`),
    INDEX `idx_redType` (`redType`),
    INDEX `idx_reciveTime` (`reciveTime`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    AUTO_INCREMENT = 1000
    COMMENT ='红包接收记录表';

DROP TABLE IF EXISTS `pet_ornament`;
CREATE TABLE `pet_ornament`
(
    `id`         INT UNSIGNED     NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `rid`        INT UNSIGNED     NOT NULL
        COMMENT '所属角色id',
    `locked`      boolean NOT NULL DEFAULT 0
        COMMENT '锁定？',
    `typeId`     TINYINT UNSIGNED  NOT NULL
        COMMENT '类型id',
    `grade`      TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '品阶',
    `place`      INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '位置，等于0-未装备, 大于0-装备了，则为宠物ID',
    `baseAttrs`  VARCHAR(2000)    NOT NULL DEFAULT ''
        COMMENT '基础属性',
    `recast`     VARCHAR(2000)    NOT NULL DEFAULT ''
        COMMENT '重铸预览数据',
    `createTime` INT UNSIGNED     NOT NULL
        COMMENT '创建时间',
    PRIMARY KEY (`id`),
    INDEX `idx_rid` (`rid`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='宠物配饰表';

DROP TABLE IF EXISTS `cd_key`;
CREATE TABLE `cd_key`
(
    `id`         INT UNSIGNED       NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `key`        VARCHAR(20)        NOT NULL
        COMMENT 'Key值',
    `typeId`     TINYINT UNSIGNED   NOT NULL
        COMMENT '类型, 1充值 2积分单冲 3物品',
    `price`      INT UNSIGNED       NOT NULL
        COMMENT '面额，单位元',
    `extra`      VARCHAR(2000)      NOT NULL DEFAULT ''
        COMMENT '附加信息',
    `createTime` INT UNSIGNED       NOT NULL DEFAULT 0
        COMMENT '创建时间',
    `doneTime`   INT UNSIGNED       NOT NULL
        COMMENT '兑换时间',
    PRIMARY KEY (`id`),
    UNIQUE KEY `ikey` (`key`) USING BTREE
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='CDKey表';
