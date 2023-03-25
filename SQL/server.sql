DROP DATABASE IF EXISTS ddxy;
CREATE DATABASE IF NOT EXISTS ddxy
    DEFAULT CHARSET utf8mb4
    COLLATE utf8mb4_unicode_ci;
USE ddxy;

SET NAMES utf8mb4;


DROP TABLE IF EXISTS `server`;
CREATE TABLE `server`
(
    `id`         INT UNSIGNED     NOT NULL AUTO_INCREMENT
        COMMENT 'id',
    `name`       VARCHAR(32)      NOT NULL
        COMMENT '区服名称',
    `status`     TINYINT UNSIGNED NOT NULL DEFAULT 0
        COMMENT '区服状态, 0-正常,1-临时维护,2-永久停服',
    `recom`      BIT              NOT NULL DEFAULT FALSE
        COMMENT '是否为推荐',
    `rank`       INT UNSIGNED     NOT NULL DEFAULT 0
        COMMENT '排序值, 越小越靠前',
    `addr`       VARCHAR(200)     NOT NULL
        COMMENT '网关地址',
    `createTime` INT UNSIGNED     NOT NULL
        COMMENT '创建时间',
    PRIMARY KEY (`id`),
    UNIQUE KEY `uk_name` (`name`)
)
    ENGINE = InnoDB
    DEFAULT CHARSET = utf8mb4
    AUTO_INCREMENT = 1000
    COMMENT ='区服表';
