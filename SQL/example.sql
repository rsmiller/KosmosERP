WITH RECURSIVE month_series AS (
    SELECT DATE_FORMAT(DATE_SUB(CURDATE(), INTERVAL 11 MONTH), '%Y-%m-01') AS month_start
    UNION ALL
    SELECT DATE_ADD(month_start, INTERVAL 1 MONTH)
    FROM month_series
    WHERE month_start < DATE_FORMAT(CURDATE(), '%Y-%m-01')
),
monthly_sales AS (
    SELECT 
        DATE_FORMAT(oh.order_date, '%Y-%m') AS month_year,
        SUM(oh.price) AS total_sales,
        COUNT(*) AS order_count,
        AVG(oh.price) AS average_order_value
    FROM order_headers oh
    WHERE oh.order_type = 'R'
        AND oh.is_deleted = 0
        AND oh.order_date >= DATE_SUB(CURDATE(), INTERVAL 12 MONTH)
    GROUP BY DATE_FORMAT(oh.order_date, '%Y-%m')
)
SELECT 
    DATE_FORMAT(ms.month_start, '%Y-%m') AS month_year,
    YEAR(ms.month_start) AS year,
    MONTH(ms.month_start) AS month,
    MONTHNAME(ms.month_start) AS month_name,
    COALESCE(msales.total_sales, 0) AS total_sales,
    COALESCE(msales.order_count, 0) AS order_count,
    COALESCE(msales.average_order_value, 0) AS average_order_value
FROM month_series ms
LEFT JOIN monthly_sales msales ON DATE_FORMAT(ms.month_start, '%Y-%m') = msales.month_year
ORDER BY 
    year DESC,
    month DESC;