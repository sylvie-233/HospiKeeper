-- =============================================================
-- 民宿管理系统 v1 · MySQL 数据库设计
-- 对应原型：Docs/酒店管理系统-原型-v1.html
-- 功能覆盖：
--   基础信息   ：房型管理、房间管理（楼层/床位/标价/底价/押金/带窗/独立卫浴/六种房态）
--   账号权限   ：操作员账号（前台/管理员）、启用停用、修改密码、最后登录时间
--   房态看板   ：房间状态实时维护（空闲/已入住/预定/打扫中/维修/停用）
--   预定管理   ：新增预定、待到店/转入住/取消
--   入住登记   ：散客入住（议价、押金、预计退房）、同住人登记、加钟
--   在住管理   ：在住列表、追加消费（矿泉水/泡面等杂费）
--   退房结算   ：房费核算、损坏扣费、押金抵扣、应补收(+)/应退款(-)
--   记录查询   ：入住记录（历史所有入住单）、退房账单明细
--   操作日志   ：谁在何时做了什么（开房/退房/预定/打扫/改价等）
--   统计报表   ：日营收/开房数/退房数汇总表（支撑当日营收、环比、30日区间查询）
--
-- 环境：MySQL 5.7+ / 8.0，引擎 InnoDB，字符集 utf8mb4
-- 约定：
--   · 所有状态用英文编码存储，中文含义见字段 COMMENT
--   · 业务单号（I/S/R 前缀）由应用层按「前缀+日期+当日序号」生成，表内唯一索引兜底
--   · 金额单位：元（DECIMAL(10,2)）；结算差额 diff > 0 应补收，< 0 应退款
-- =============================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- -------------------------------------------------------------
-- 1. 操作员账号表 sys_user
--    功能：账号密码登录、两种角色权限、启用/停用、修改密码
-- -------------------------------------------------------------
DROP TABLE IF EXISTS `sys_user`;
CREATE TABLE `sys_user` (
  `id`          INT     NOT NULL AUTO_INCREMENT COMMENT '主键',
  `username`    VARCHAR(32)      NOT NULL                COMMENT '登录账号（唯一）',
  `password`    CHAR(64)         NOT NULL                COMMENT '密码 SHA-256 摘要（demo 明文 123456）',
  `real_name`   VARCHAR(32)      NOT NULL                COMMENT '姓名，如：老陈 / 小王',
  `role`        ENUM('FRONT','ADMIN') NOT NULL DEFAULT 'FRONT' COMMENT '角色：FRONT=前台，ADMIN=管理员',
  `status`      ENUM('ACTIVE','DISABLED') NOT NULL DEFAULT 'ACTIVE' COMMENT '状态：ACTIVE=启用，DISABLED=停用',
  `last_login`  DATETIME                  NULL           COMMENT '最后登录时间',
  `created_at`  DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at`  DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_username` (`username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='操作员账号（前台/管理员）';

-- -------------------------------------------------------------
-- 2. 房型表 room_type
--    功能：房型名称、基础定价、备注；房型改名需同步房间冗余字段（应用层处理）
-- -------------------------------------------------------------
DROP TABLE IF EXISTS `room_type`;
CREATE TABLE `room_type` (
  `id`          INT     NOT NULL AUTO_INCREMENT COMMENT '主键',
  `name`        VARCHAR(32)      NOT NULL                COMMENT '房型名称：单人间/双人间/大床房',
  `base_price`  DECIMAL(10,2)    NOT NULL DEFAULT 0.00   COMMENT '基础定价（元/晚）',
  `note`        VARCHAR(255)              NULL           COMMENT '备注',
  `created_at`  DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at`  DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='房型管理';

-- -------------------------------------------------------------
-- 3. 房间表 room
--    功能：房间增删改（房号/楼层/房型/床位/标价/底价/押金/带窗/独立卫浴）、六种房态
--    房态看板按 floor + room_type_id + status 筛选渲染
-- -------------------------------------------------------------
DROP TABLE IF EXISTS `room`;
CREATE TABLE `room` (
  `id`          INT     NOT NULL AUTO_INCREMENT COMMENT '主键',
  `room_no`     VARCHAR(16)      NOT NULL                COMMENT '房号，如 101 / 205（唯一）',
  `floor`       INT              NOT NULL DEFAULT 1      COMMENT '楼层（看板/筛选维度）',
  `type_id`     INT     NOT NULL                COMMENT '房型 ID → room_type.id',
  `type_name`   VARCHAR(32)      NOT NULL                COMMENT '房型名称冗余（看板直读，房型改名时同步更新）',
  `beds`        TINYINT UNSIGNED NOT NULL DEFAULT 1      COMMENT '床位数',
  `price`       DECIMAL(10,2)    NOT NULL DEFAULT 0.00   COMMENT '标价（元/晚）',
  `min_price`   DECIMAL(10,2)    NOT NULL DEFAULT 0.00   COMMENT '底价（议价下限校验）',
  `deposit`     DECIMAL(10,2)    NOT NULL DEFAULT 0.00   COMMENT '押金标准（元）',
  `has_window`  TINYINT(1)       NOT NULL DEFAULT 1      COMMENT '是否带窗：1=是 0=否',
  `has_bath`    TINYINT(1)       NOT NULL DEFAULT 1      COMMENT '是否独立卫浴：1=是 0=否',
  `status`      ENUM('FREE','OCCUPIED','RESERVED','CLEANING','REPAIR','DISABLED')
                                 NOT NULL DEFAULT 'FREE' COMMENT '房态：FREE=空闲 OCCUPIED=已入住 RESERVED=预定 CLEANING=打扫中 REPAIR=维修 DISABLED=停用',
  `note`        VARCHAR(255)              NULL           COMMENT '备注（如维修原因）',
  `created_at`  DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at`  DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_room_no` (`room_no`),
  KEY `idx_floor` (`floor`),
  KEY `idx_type` (`type_id`),
  KEY `idx_status` (`status`),
  CONSTRAINT `fk_room_type` FOREIGN KEY (`type_id`) REFERENCES `room_type` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='房间（房态看板核心）';

-- -------------------------------------------------------------
-- 4. 预定表 reservation
--    功能：新增预定、待到店(PENDING)、一键转入住(CONVERTED)、取消(CANCELLED)
-- -------------------------------------------------------------
DROP TABLE IF EXISTS `reservation`;
CREATE TABLE `reservation` (
  `id`           INT    NOT NULL AUTO_INCREMENT COMMENT '主键',
  `res_no`       VARCHAR(32)     NOT NULL                COMMENT '预定单号：R+日期+序号，如 R20261125001',
  `guest_name`   VARCHAR(32)     NOT NULL                COMMENT '客人姓名',
  `guest_phone`  VARCHAR(20)     NOT NULL                COMMENT '联系电话',
  `guest_id_card` VARCHAR(18)              NULL           COMMENT '游客身份证号（Web 预订选填）',
  `type_id`      INT             NOT NULL                COMMENT '预定房型 → room_type.id（房型库存模型）',
  `room_id`      INT                      NULL           COMMENT '到店分配的房间 → room.id；游客在线预订提交时为 NULL',
  `room_no`      VARCHAR(16)              NULL           COMMENT '房号冗余；游客在线预订提交时为 NULL，转入住时回填',
  `room_type_name` VARCHAR(32)   NOT NULL                COMMENT '房型名称冗余（列表直读）',
  `check_in_date`  DATE          NOT NULL                COMMENT '入住日期',
  `check_out_date` DATE          NOT NULL                COMMENT '退房日期，必须晚于入住日期',
  `channel`      ENUM('WALK_IN','WEB') NOT NULL DEFAULT 'WALK_IN' COMMENT '预订渠道：前台 / 游客 Web',
  `unit_price`   DECIMAL(10,2)             NULL           COMMENT '下单时房型单价快照',
  `arrive_time`  DATETIME        NOT NULL                COMMENT '预计到店时间',
  `status`       ENUM('PENDING','CONVERTED','CANCELLED') NOT NULL DEFAULT 'PENDING'
                                                         COMMENT '状态：PENDING=待到店 CONVERTED=已转入住 CANCELLED=已取消',
  `checkin_id`   INT             NULL           COMMENT '转入住后的入住单 → checkin_record.id（未转为 NULL）',
  `note`         VARCHAR(255)             NULL           COMMENT '备注',
  `op_user_id`   INT             NULL           COMMENT '经办人 → sys_user.id',
  `created_at`   DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at`   DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_res_no` (`res_no`),
  KEY `idx_status` (`status`),
  KEY `idx_arrive` (`arrive_time`),
  KEY `idx_phone` (`guest_phone`),
  KEY `idx_type_dates` (`type_id`, `check_in_date`, `check_out_date`),
  CONSTRAINT `fk_res_type` FOREIGN KEY (`type_id`) REFERENCES `room_type` (`id`),
  CONSTRAINT `fk_res_room` FOREIGN KEY (`room_id`) REFERENCES `room` (`id`),
  CONSTRAINT `fk_res_checkin` FOREIGN KEY (`checkin_id`) REFERENCES `checkin_record` (`id`),
  CONSTRAINT `fk_res_user` FOREIGN KEY (`op_user_id`) REFERENCES `sys_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='预定管理（前台代客 + 游客 Web；Web 预订只锁房型日期段，不锁房号）';

-- -------------------------------------------------------------
-- 5. 入住单表 checkin_record（核心）
--    功能：散客入住登记、议价（≥底价校验在应用层）、押金、预计退房时间、
--          加钟（extend_hours>0 时按小时计费）、状态流转 在住→已退房
--    查询：按房号/姓名/手机号/时间段检索（idx_room_no / idx_phone / idx_checkin_time）
-- -------------------------------------------------------------
DROP TABLE IF EXISTS `checkin_record`;
CREATE TABLE `checkin_record` (
  `id`            INT   NOT NULL AUTO_INCREMENT COMMENT '主键',
  `checkin_no`    VARCHAR(32)    NOT NULL                COMMENT '入住单号：I+日期+序号，如 I20261125001',
  `room_id`       INT   NOT NULL                COMMENT '房间 → room.id',
  `room_no`       VARCHAR(16)    NOT NULL                COMMENT '房号冗余（列表直读/检索）',
  `guest_name`    VARCHAR(32)    NOT NULL                COMMENT '主客人姓名',
  `guest_phone`   VARCHAR(20)    NOT NULL                COMMENT '主客人手机号',
  `guest_idcard`  VARCHAR(18)    NOT NULL                COMMENT '主客人身份证号',
  `checkin_time`  DATETIME       NOT NULL                COMMENT '实际入住时间',
  `plan_out_time` DATETIME       NOT NULL                COMMENT '预计退房时间（默认次日 12:00）',
  `actual_out_time` DATETIME              NULL           COMMENT '实际退房时间（退房时写入）',
  `price`         DECIMAL(10,2)  NOT NULL                COMMENT '成交单价（议价后，≥房间底价）',
  `origin_price`  DECIMAL(10,2)           NULL           COMMENT '议价前标价（NULL=未议价）',
  `deposit`       DECIMAL(10,2)  NOT NULL DEFAULT 0.00   COMMENT '已收押金（元）',
  `extend_hours`  INT            NOT NULL DEFAULT 0      COMMENT '加钟小时数（>0 退房按小时计费）',
  `status`        ENUM('LIVING','CHECKED_OUT') NOT NULL DEFAULT 'LIVING'
                                                        COMMENT '状态：LIVING=在住 CHECKED_OUT=已退房',
  `note`          VARCHAR(255)            NULL           COMMENT '备注',
  `op_user_id`    INT            NULL           COMMENT '经办人 → sys_user.id',
  `created_at`    DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at`    DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_checkin_no` (`checkin_no`),
  KEY `idx_room_no` (`room_no`),
  KEY `idx_guest_name` (`guest_name`),
  KEY `idx_phone` (`guest_phone`),
  KEY `idx_idcard` (`guest_idcard`),
  KEY `idx_checkin_time` (`checkin_time`),
  KEY `idx_status` (`status`),
  CONSTRAINT `fk_ci_room` FOREIGN KEY (`room_id`) REFERENCES `room` (`id`),
  CONSTRAINT `fk_ci_user` FOREIGN KEY (`op_user_id`) REFERENCES `sys_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='入住单（散客入住/在住/历史记录）';

-- -------------------------------------------------------------
-- 5.1 同住人表 checkin_companion
--     功能：一张入住单支持登记多名同住人
-- -------------------------------------------------------------
DROP TABLE IF EXISTS `checkin_companion`;
CREATE TABLE `checkin_companion` (
  `id`           INT    NOT NULL AUTO_INCREMENT COMMENT '主键',
  `checkin_id`   INT    NOT NULL                COMMENT '入住单 → checkin_record.id',
  `name`         VARCHAR(32)     NOT NULL                COMMENT '同住人姓名',
  `phone`        VARCHAR(20)              NULL           COMMENT '手机号',
  `idcard`       VARCHAR(18)              NULL           COMMENT '身份证号',
  `created_at`   DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '登记时间',
  PRIMARY KEY (`id`),
  KEY `idx_checkin` (`checkin_id`),
  CONSTRAINT `fk_cc_checkin` FOREIGN KEY (`checkin_id`) REFERENCES `checkin_record` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='同住人登记';

-- -------------------------------------------------------------
-- 5.2 追加消费表 extra_charge
--     功能：入住期间杂费录入（矿泉水、泡面等），退房结算时汇总进账单
-- -------------------------------------------------------------
DROP TABLE IF EXISTS `extra_charge`;
CREATE TABLE `extra_charge` (
  `id`           INT    NOT NULL AUTO_INCREMENT COMMENT '主键',
  `checkin_id`   INT    NOT NULL                COMMENT '入住单 → checkin_record.id',
  `item`         VARCHAR(64)     NOT NULL                COMMENT '消费项目，如：矿泉水 / 泡面',
  `amount`       DECIMAL(10,2)   NOT NULL DEFAULT 0.00   COMMENT '金额（元）',
  `charge_time`  DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '消费时间',
  `op_user_id`   INT             NULL           COMMENT '经办人 → sys_user.id',
  PRIMARY KEY (`id`),
  KEY `idx_checkin` (`checkin_id`),
  CONSTRAINT `fk_ec_checkin` FOREIGN KEY (`checkin_id`) REFERENCES `checkin_record` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_ec_user` FOREIGN KEY (`op_user_id`) REFERENCES `sys_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='入住期间追加消费（杂费）';

-- -------------------------------------------------------------
-- 6. 退房结算账单表 checkout_bill
--    功能：退房结算明细票据。计算口径：
--      room_fee   = 房费（按天计费；extend_hours>0 时按 标价/24/小时 向上取整）
--      extra_total= 追加消费合计（extra_charge 汇总快照）
--      damage     = 物品损坏扣费
--      diff = room_fee + extra_total + damage − deposit_received
--             diff > 0 应补收，diff < 0 应退款(-)退还押金余款
-- -------------------------------------------------------------
DROP TABLE IF EXISTS `checkout_bill`;
CREATE TABLE `checkout_bill` (
  `id`            INT   NOT NULL AUTO_INCREMENT COMMENT '主键',
  `bill_no`       VARCHAR(32)    NOT NULL                COMMENT '结算单号：S+日期+序号，如 S20261123002',
  `checkin_id`    INT   NOT NULL                COMMENT '入住单 → checkin_record.id（1:1）',
  `room_id`       INT   NOT NULL                COMMENT '房间 → room.id',
  `room_no`       VARCHAR(16)    NOT NULL                COMMENT '房号冗余',
  `guest_name`    VARCHAR(32)    NOT NULL                COMMENT '客人姓名（快照）',
  `checkin_time`  DATETIME       NOT NULL                COMMENT '入住时间（快照）',
  `checkout_time` DATETIME       NOT NULL                COMMENT '实际退房时间',
  `nights`        INT            NOT NULL DEFAULT 1      COMMENT '计费晚数（加钟场景可为 0 纯钟点）',
  `room_fee`      DECIMAL(10,2)  NOT NULL DEFAULT 0.00   COMMENT '房费（元）',
  `extra_total`   DECIMAL(10,2)  NOT NULL DEFAULT 0.00   COMMENT '追加消费合计（元）',
  `damage`        DECIMAL(10,2)  NOT NULL DEFAULT 0.00   COMMENT '物品损坏扣费（元）',
  `other_fee`     DECIMAL(10,2)  NOT NULL DEFAULT 0.00   COMMENT '其他消费（元，简易杂费兜底字段）',
  `deposit_received` DECIMAL(10,2) NOT NULL DEFAULT 0.00 COMMENT '已收押金（元）',
  `deposit_used`  DECIMAL(10,2)  NOT NULL DEFAULT 0.00   COMMENT '押金抵扣金额（元）',
  `total`         DECIMAL(10,2)  NOT NULL DEFAULT 0.00   COMMENT '应收合计 = room_fee + extra_total + damage + other_fee',
  `diff`          DECIMAL(10,2)  NOT NULL DEFAULT 0.00   COMMENT '结算差额：>0 应补收，<0 应退款，=0 押金正好抵扣',
  `note`          VARCHAR(255)            NULL           COMMENT '备注（损坏说明等）',
  `op_user_id`    INT            NULL           COMMENT '结算经办人 → sys_user.id',
  `created_at`    DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '结算时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_bill_no` (`bill_no`),
  UNIQUE KEY `uk_checkin` (`checkin_id`),
  KEY `idx_room_no` (`room_no`),
  KEY `idx_guest` (`guest_name`),
  KEY `idx_checkout_time` (`checkout_time`),
  CONSTRAINT `fk_bill_checkin` FOREIGN KEY (`checkin_id`) REFERENCES `checkin_record` (`id`),
  CONSTRAINT `fk_bill_room` FOREIGN KEY (`room_id`) REFERENCES `room` (`id`),
  CONSTRAINT `fk_bill_user` FOREIGN KEY (`op_user_id`) REFERENCES `sys_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='退房结算账单';

-- -------------------------------------------------------------
-- 7. 操作日志表 op_log
--    功能：谁在什么时候办理开房/退房/预定/打扫/改价等（前台只能看自己的）
-- -------------------------------------------------------------
DROP TABLE IF EXISTS `op_log`;
CREATE TABLE `op_log` (
  `id`           BIGINT NOT NULL AUTO_INCREMENT COMMENT '主键',
  `user_id`      INT             NULL           COMMENT '操作人 → sys_user.id',
  `user_name`    VARCHAR(64)              NULL           COMMENT '操作人姓名冗余（含角色），如：小王（前台）',
  `action`       VARCHAR(32)     NOT NULL                COMMENT '动作类型：开房/退房/预定/取消预定/打扫/改价/登录 等',
  `target`       VARCHAR(64)              NULL           COMMENT '操作对象，如：201 · 大床房 / 房型·大床房',
  `detail`       VARCHAR(500)             NULL           COMMENT '明细描述，如：刘洋 入住，议价 ¥198，押金 ¥300',
  `biz_no`       VARCHAR(32)              NULL           COMMENT '关联业务单号（入住单/结算单/预定单号）',
  `created_at`   DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '操作时间',
  PRIMARY KEY (`id`),
  KEY `idx_user` (`user_id`),
  KEY `idx_action` (`action`),
  KEY `idx_created` (`created_at`),
  CONSTRAINT `fk_log_user` FOREIGN KEY (`user_id`) REFERENCES `sys_user` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='操作日志';

-- -------------------------------------------------------------
-- 8. 日营收统计表 daily_revenue_stat（统计报表支撑）
--    功能：按天汇总营收/开房数/退房数，支撑：
--      · 今日营收 + 环比（与昨日对比）
--      · 近 30 日柱状图 / 时间段营收区间查询（含自定义起止日期）
--      · 本月累计营收
--    生成方式：退房结算落账单时同步 UPDATE 当日记录；夜间可用定时任务校准
-- -------------------------------------------------------------
DROP TABLE IF EXISTS `daily_revenue_stat`;
CREATE TABLE `daily_revenue_stat` (
  `id`             INT  NOT NULL AUTO_INCREMENT COMMENT '主键',
  `stat_date`      DATE          NOT NULL                COMMENT '统计日期（唯一）',
  `revenue`        DECIMAL(12,2) NOT NULL DEFAULT 0.00   COMMENT '当日营收合计（元）',
  `checkin_count`  INT           NOT NULL DEFAULT 0      COMMENT '当日开房数量',
  `checkout_count` INT           NOT NULL DEFAULT 0      COMMENT '当日退房数量',
  `occupancy_rate` DECIMAL(5,2)  NOT NULL DEFAULT 0.00   COMMENT '当日平均入住率（%，可夜间任务回填）',
  `updated_at`     DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_stat_date` (`stat_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='日营收统计（报表/环比/30日查询）';

SET FOREIGN_KEY_CHECKS = 1;

-- =============================================================
-- 初始化数据（与原型演示数据一致）
-- =============================================================

-- 操作员账号（密码统一为 123456 的 SHA-256）
INSERT INTO `sys_user` (`id`,`username`,`password`,`real_name`,`role`,`status`,`last_login`) VALUES
(1,'admin',  SHA2('123456',256),'老陈','ADMIN','ACTIVE','2026-11-25 09:28:00'),
(2,'front01',SHA2('123456',256),'小王','FRONT','ACTIVE','2026-11-25 08:00:00'),
(3,'front02',SHA2('123456',256),'小李','FRONT','DISABLED','2026-11-24 22:30:00');

-- 房型
INSERT INTO `room_type` (`id`,`name`,`base_price`,`note`) VALUES
(1,'单人间',128.00,'小面积，适合独行客人'),
(2,'双人间',168.00,'标准双床'),
(3,'大床房',198.00,'1.8m 大床，情侣/夫妻偏好');

-- 房间（14 间，含六种房态演示）
INSERT INTO `room` (`id`,`room_no`,`floor`,`type_id`,`type_name`,`beds`,`price`,`min_price`,`deposit`,`has_window`,`has_bath`,`status`) VALUES
(1 ,'101','1',1,'单人间',1,128.00,100.00,100.00,1,1,'FREE'),
(2 ,'102','1',2,'双人间',2,168.00,130.00,200.00,1,1,'OCCUPIED'),
(3 ,'103','1',3,'大床房',2,198.00,160.00,200.00,1,1,'OCCUPIED'),
(4 ,'104','1',1,'单人间',1,128.00,100.00,100.00,1,1,'CLEANING'),
(5 ,'105','1',2,'双人间',2,168.00,130.00,200.00,0,1,'RESERVED'),
(6 ,'106','1',2,'双人间',2,168.00,130.00,200.00,0,1,'REPAIR'),
(7 ,'201','2',3,'大床房',2,198.00,160.00,300.00,1,1,'OCCUPIED'),
(8 ,'202','2',3,'大床房',2,198.00,160.00,200.00,1,1,'OCCUPIED'),
(9 ,'203','2',2,'双人间',2,168.00,130.00,200.00,1,1,'FREE'),
(10,'204','2',1,'单人间',1,128.00,100.00,100.00,1,0,'FREE'),
(11,'205','2',2,'双人间',2,168.00,130.00,200.00,1,1,'FREE'),
(12,'206','2',3,'大床房',2,198.00,160.00,300.00,1,1,'RESERVED'),
(13,'207','2',3,'大床房',2,198.00,160.00,300.00,1,1,'CLEANING'),
(14,'208','2',1,'单人间',1,128.00,100.00,100.00,0,0,'DISABLED');

-- 预定（演示：2 条待到店）
INSERT INTO `reservation` (`res_no`,`guest_name`,`guest_phone`,`type_id`,`room_id`,`room_no`,`room_type_name`,`check_in_date`,`check_out_date`,`channel`,`arrive_time`,`status`,`op_user_id`) VALUES
('R20261125001','王芳','13900005678',2,5 ,'105','双人间','2026-11-25','2026-11-26','WALK_IN','2026-11-25 18:00:00','PENDING',2),
('R20261125002','孙敏','13700009012',3,12,'206','大床房','2026-11-26','2026-11-27','WALK_IN','2026-11-26 14:00:00','PENDING',2);

-- =============================================================
-- 常用查询示例（对应原型各页面）
-- =============================================================
-- · 房态看板：SELECT * FROM room WHERE (floor = ? OR ? IS NULL) AND (type_id = ? OR ? IS NULL) ORDER BY floor, room_no;
-- · 在住列表：SELECT * FROM checkin_record WHERE status = 'LIVING' ORDER BY checkin_time DESC;
-- · 入住记录检索（房号/姓名/手机号/时间段）：
--   SELECT * FROM checkin_record
--   WHERE room_no LIKE CONCAT('%',?,'%') AND guest_name LIKE CONCAT('%',?,'%')
--     AND guest_phone LIKE CONCAT('%',?,'%') AND checkin_time BETWEEN ? AND ?;
-- · 退房结算差额示例（与原型 204 周杰一致：房费128 + 消费0 + 损坏0 − 押金100 = 应补收 28）：
--   diff = 128.00 + 0.00 + 0.00 - 100.00 = 28.00 → 应补收 ¥28
-- · 时间段营收（自定义区间）：SELECT stat_date, revenue, checkin_count, checkout_count
--   FROM daily_revenue_stat WHERE stat_date BETWEEN ? AND ? ORDER BY stat_date;
-- · 今日营收环比：今日 revenue 与 stat_date = 昨日 的 revenue 对比。
