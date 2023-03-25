USE ddxy;

SET NAMES utf8mb4;

DROP TABLE IF EXISTS `admin`;
CREATE TABLE `admin`
(
    `id`         INT UNSIGNED     NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `username`   VARCHAR(64)      NOT NULL
        COMMENT '用户名',
    `password`   VARCHAR(64)      NOT NULL
        COMMENT '用户密码',
    `passSalt`   VARCHAR(64)      NOT NULL
        COMMENT '密码盐',
    `nickname`   VARCHAR(32)      NOT NULL DEFAULT ''
        COMMENT '用户昵称',
    `status`     TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '账号状态, 0-正常, 1-冻结',
    `category`   TINYINT UNSIGNED NOT NULL DEFAULT 2
        COMMENT '账号类型, 1-超级管理员, 2-管理员, 3-代理',
    `money`      INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '余额',
    `totalPay`   INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '总充值额度',
    `invitCode`  VARCHAR(12)      NOT NULL DEFAULT ''
        COMMENT '邀请码',
    `parentId`   INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '所属代理id, 0表示管理员',
    `agency`     INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '代理等级, 0表示管理员',
    `loginIp`    VARCHAR(200)     NOT NULL DEFAULT ''
        COMMENT '上次登录IP',
    `loginTime`  INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '上次登录时间',
    `createTime` INT UNSIGNED     NOT NULL
        COMMENT '创建时间',
    PRIMARY KEY (`id`),
    UNIQUE KEY `idx_username` (`username`),
    UNIQUE KEY `idx_nickname` (`nickname`),
    UNIQUE KEY `idx_invit_code` (`invitCode`),
    INDEX `idx_category` (`category`),
    INDEX `idx_parentId` (`parentId`),
    INDEX `idx_agency` (`agency`),
    INDEX `idx_createTime` (`createTime`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    AUTO_INCREMENT = 1000
    COMMENT ='管理后台账号表';

-- dhxy dhxy123456
INSERT INTO `admin`
VALUES (1, 'dhxy', 'ceMJbPXoMmwUSmq7TTQVSEZnP6M4W6IQ6xlDNUkKByg=',
        'MTwnk2V6BYGMyB6L1U5v6Q==', '系统管理员', 0, 1, 0, 0, '123456', 0, 0, '', 0, 1606028199);



DROP TABLE IF EXISTS `recharge`;
CREATE TABLE `recharge`
(
    `id`         INT UNSIGNED NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `operator`   INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '操作的管理员id',
    `from`       INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '源id,如果是管理员则为0',
    `to`         INT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '目标代理id',
    `money`      INT          NOT NULL DEFAULT 0
        COMMENT '充值额度',
    `remark`     VARCHAR(500) NOT NULL DEFAULT ''
        COMMENT '备注',
    `createTime` INT UNSIGNED NOT NULL
        COMMENT '创建时间',
    PRIMARY KEY (`id`),
    INDEX `idx_from` (`from`),
    INDEX `idx_to` (`to`),
    INDEX `idx_createTime` (`createTime`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='代理充值表';


DROP TABLE IF EXISTS `error`;
CREATE TABLE `error`
(
    `id`         INT UNSIGNED     NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `uid`        INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '用户id',
    `rid`        INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '角色id',
    `error`      VARCHAR(3000)    NOT NULL DEFAULT ''
        COMMENT '错误详情',
    `remark`     VARCHAR(500)     NOT NULL DEFAULT ''
        COMMENT '备注',
    `status`     TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '0-未处理,1-已处理,2-忽略,3-延迟',
    `createTime` INT UNSIGNED     NOT NULL
        COMMENT '创建时间',
    PRIMARY KEY (`id`),
    INDEX `idx_uid` (`uid`),
    INDEX `idx_rid` (`rid`),
    INDEX `idx_status` (`status`),
    INDEX `idx_createTime` (`createTime`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    COMMENT ='错误信息表';
