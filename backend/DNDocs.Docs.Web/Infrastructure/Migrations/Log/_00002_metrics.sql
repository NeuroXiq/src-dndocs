BEGIN TRANSACTION;



create table mt_instrument
(
id integer primary key autoincrement,
[name] text,
[meter_name] text,
instance_id text,
created_on text,
tags text,
[type] int
);

create table mt_hrange
(
id integer primary key autoincrement,
mt_instrument_id int,
[end] real
);


create table mt_measurement
(
id integer primary key autoincrement,
mt_instrument_id int,
[value] real,
mt_hrange_id int,
created_on text
);

COMMIT;