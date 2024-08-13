BEGIN TRANSACTION;

create table app_log
(
 id integer primary key autoincrement,
[message] text,
category_name text,
log_level_id int,
[date] text
);

COMMIT;