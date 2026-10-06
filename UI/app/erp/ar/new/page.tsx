import { redirect } from 'next/navigation';

/**
 * Invoices are created from an order: AR list > "Create Invoice" opens
 * /erp/ar/new/[orderGuid]. This route used to be a stub with hard-coded sample
 * rows (BUG-013), so it now sends anyone who lands here to the AR list.
 */
export default function NewARInvoicePage() {
  redirect('/erp/ar');
}
