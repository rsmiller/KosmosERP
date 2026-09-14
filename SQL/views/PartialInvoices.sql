
CREATE OR REPLACE
ALGORITHM = UNDEFINED VIEW vw_PartialInvoices AS
-- Partial Invoices
SELECT *
FROM (
	SELECT
		aih.id ,
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
	    END) AS pay_method_name,
		(
			SELECT SUM(ol.quantity)
			FROM order_lines ol
			WHERE ol.order_header_id = oh.id
			AND ol.is_deleted = 0
		) as sold_qty,
		(
			SELECT SUM(ail.invoice_qty)
			FROM ar_invoice_lines ail
			WHERE ail.ar_invoice_header_id  = aih.id
			AND ail.is_deleted = 0
		) as invoiced_qty
	FROM ar_invoice_headers aih
	JOIN order_headers oh ON oh.id = aih.order_header_id
	JOIN customers c ON oh.customer_id = c.id
	WHERE aih.id IN (
		SELECT invoice_id
		FROM (
			SELECT
			  (SELECT DISTINCT aih.id
			  	FROM ar_invoice_headers aih 
			  	WHERE aih.order_header_id = oh.id 
			    AND aih.is_deleted = 0
			    AND aih.is_paid = 0
			  	LIMIT 1
			  ) AS invoice_id
			FROM order_headers oh
			WHERE oh.order_type ='R'
			AND oh.is_deleted = 0
			AND oh.is_complete = 0
		) as o1
		WHERE invoice_id IS NOT NULL
	)
) o2
WHERE o2.invoiced_qty < o2.sold_qty