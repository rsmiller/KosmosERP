-- `kosmos-erp`.vw_ReadyToShip source

CREATE OR REPLACE
ALGORITHM = UNDEFINED VIEW `kosmos-erp`.`vw_ReadyToShip` AS
select
    `c`.`customer_name` AS `customer_name`,
    `o`.`order_number` AS `order_number`,
    `o`.`guid` AS `order_guid`,
    `p`.`product_name` AS `product_name`,
    `ol`.`quantity` AS `sold_quantity`,
    sum(`pol`.`quantity`) AS `produced_quantity`,
    ifnull((select sum(`sl`.`units_shipped`) AS `units_shipped` from (`kosmos-erp`.`shipment_lines` `sl` join `kosmos-erp`.`shipment_headers` `sh` on((`sh`.`id` = `sl`.`shipment_header_id`))) where (`sl`.`order_line_id` = `ol`.`id`) group by `sl`.`units_shipped`), 0) AS `shipped_quantity`
from
    ((((`kosmos-erp`.`production_order_lines` `pol`
join `kosmos-erp`.`order_lines` `ol` on
    ((`ol`.`id` = `pol`.`order_line_id`)))
join `kosmos-erp`.`order_headers` `o` on
    ((`ol`.`order_header_id` = `o`.`id`)))
join `kosmos-erp`.`customers` `c` on
    ((`c`.`id` = `o`.`customer_id`)))
join `kosmos-erp`.`products` `p` on
    ((`p`.`id` = `ol`.`product_id`)))
where
    (`pol`.`status` = 'production_order_status_ready_to_ship')
group by
    `c`.`customer_name`,
    `o`.`order_number`,
    `o`.`guid`,
    `ol`.`product_id`,
    `p`.`product_name`,
    `ol`.`line_number`,
    `ol`.`quantity`,
    `ol`.`id`;