BEGIN TRANSACTION;

create index ix_mt_measurement_created_on on mt_measurement(created_on);

COMMIT;