
CREATE OR REPLACE
ALGORITHM = UNDEFINED VIEW vw_OrdersReadyForInvoicing AS
-- Orders Without Any Invoices
SELECT
	order_number AS order_number,
    order_date AS order_date,
    oh.created_by AS created_by,
    customer_name AS customer_name,
    c.guid AS customer_guid,
    oh.guid AS order_guid,
    pay_method AS pay_method,
	(CASE 
		WHEN oh.pay_method = 'payment_method_cash' THEN 'Cash'
        WHEN oh.pay_method = 'payment_method_card' THEN 'Card'
        WHEN oh.pay_method = 'payment_method_po' THEN 'PO'
    END) AS pay_method_name
FROM order_headers oh
JOIN customers c ON oh.customer_id = c.id
WHERE oh.id IN (
	SELECT
		DISTINCT
		COALESCE(
	    (
	      SELECT aih.id
	      FROM ar_invoice_headers aih 
	      WHERE aih.order_header_id = oh.id 
	        AND aih.is_deleted = 0
	        AND aih.is_paid = 0
	      LIMIT 1
	    ),
	    oh.id
	  ) AS matching_order_id
	FROM order_headers oh
	WHERE oh.order_type ='R'
	AND oh.is_deleted = 0
	AND oh.is_complete = 0
)