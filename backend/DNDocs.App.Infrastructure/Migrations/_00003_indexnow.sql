BEGIN TRANSACTION;

drop table app_setting;
create table sys_var
(
id integer primary key,
[key] text,
[value] text
);

create table indexnow_log
(
id integer primary key autoincrement,
site_item_id_start int,
site_item_id_end int,
success boolean,
last_exception text,
last_submit_date text,
submit_attempt_count int
);

COMMIT;