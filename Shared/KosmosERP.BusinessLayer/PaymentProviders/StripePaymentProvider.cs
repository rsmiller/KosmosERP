
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.Payment.Dto;
using KosmosERP.BusinessLayer.PaymentProviders.Models;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.PaymentProviders;

public class StripePaymentProvider : IPaymentProvider
{
    private IPaymentProviderSettings _Settings;
 
    public StripePaymentProvider(IPaymentProviderSettings settings)
    {
        _Settings = settings;
    }

    public async Task<Response<PaymentCustomer>> GetCustomer(string external_id)
    {
        Response<PaymentCustomer> response = new Response<PaymentCustomer>();

        try
        {
            if(String.IsNullOrEmpty(external_id))
                throw new Exception("External Id cannot be null.");

            var service = new Stripe.CustomerService();
            var payment_customer = await service.GetAsync(external_id);

            if(payment_customer == null)
            {
                throw new Exception("Failed to create new customer in Stripe.");
            }
            else
            {
                response.Data = new PaymentCustomer
                {
                    identifier = payment_customer.Id,
                    success = true
                };
            }
        }
        catch(Exception ex)
        {
            response.SetException(ex.Message, ResultCode.Error);
        }
        
        return response;
    }

    public async Task<Response<PaymentCustomer>> CreateCustomer(Customer customer, Address billing_address)
    {
        Response<PaymentCustomer> response = new Response<PaymentCustomer>();

        try
        {
            var service = new Stripe.CustomerService();
            var payment_customer_update = await service.CreateAsync(new Stripe.CustomerCreateOptions
            {
                Email = customer.general_email,
                Name = customer.customer_name,
                Phone = customer.phone,
                TaxExempt = customer.is_taxable ? "none" : "exempt",
                Address = new Stripe.AddressOptions
                {
                    Line1 = billing_address.street_address1,
                    Line2 = billing_address.street_address1,
                    City = billing_address.city,
                    State = billing_address.state,
                    PostalCode = billing_address.postal_code,
                    Country = billing_address.country
                }
            });

            if(payment_customer_update == null)
                throw new Exception("Failed to update customer in Stripe.");

            response.Data = new PaymentCustomer
            {
                identifier = payment_customer_update.Id,
                success = true
            };
        }
        catch(Exception ex)
        {
            response.SetException(ex.Message, ResultCode.Error);
        }
        
        return response;
    }

    public async Task<Response<PaymentCustomer>> UpdateCustomer(Customer customer, Address billing_address)
    {
        Response<PaymentCustomer> response = new Response<PaymentCustomer>();

        try
        {
            var service = new Stripe.CustomerService();
            var payment_customer = await service.GetAsync(customer.payment_external_id);

            if(payment_customer == null)
            {
                var new_customer = await this.CreateCustomer(customer, billing_address);

                if(new_customer.Success)
                    response.Data = new_customer.Data;
                else
                    throw new Exception("Failed to create new customer in Stripe.");
            }
            else
            {
                var payment_customer_update = await service.UpdateAsync(payment_customer.Id, new Stripe.CustomerUpdateOptions
                {
                    Email = customer.general_email,
                    Name = customer.customer_name,
                    Phone = customer.phone,
                    TaxExempt = customer.is_taxable ? "none" : "exempt",
                });

                if(payment_customer_update == null)
                    throw new Exception("Failed to update customer in Stripe.");

                response.Data = new PaymentCustomer
                {
                    identifier = payment_customer_update.Id,
                    success = true
                };
            }
        }
        catch(Exception ex)
        {
            response.SetException(ex.Message, ResultCode.Error);
        }
        
        return response;
    }

    public async Task<Response<PaymentProviderTransactionCreateDto>> CreateTransaction(ARInvoiceHeader ar_invoice_header, Payment payment, Customer customer, OrderHeader order_header, Dictionary<string, string>? metadata = null)
    {
        var options = new Stripe.PaymentIntentCreateOptions
        {
            Amount = (long)(payment.payment_amount * 100),
            Currency = "usd",
            PaymentMethodTypes = new List<string> { "card", "us_bank_account" },
            Customer = customer.payment_external_id,
            Metadata = new Dictionary<string, string>
            {
                { "Invoice #", ar_invoice_header.invoice_number.ToString() },
                { "Order #", order_header.order_number.ToString() },
                { "Customer #", customer.customer_number.ToString()}
            },
        };


        if(metadata != null)
        {
            if(metadata.ContainsKey("payment_method_id"))
            {
                options.PaymentMethod = metadata["payment_method_id"];
            }
        }

        
        var service = new Stripe.PaymentIntentService();
        var paymentIntent = service.Create(options);

        var dto = new PaymentProviderTransactionCreateDto()
        {
            id = paymentIntent.Id,
            clientSecret = paymentIntent.ClientSecret,
            ar_invoice_number = ar_invoice_header.invoice_number.ToString(),
            amount = payment.payment_amount
        };

        return new Response<PaymentProviderTransactionCreateDto>(dto);
    }

    public async Task<Response<PaymentProviderTransactionGetDto>> GetTransaction(string external_id)
    {
        Response<PaymentProviderTransactionGetDto> response = new Response<PaymentProviderTransactionGetDto>();

        try
        {
            if(external_id.StartsWith("inp"))
            {
                var paymentService = new Stripe.InvoicePaymentService();
                var options = new Stripe.InvoicePaymentGetOptions();
                options.AddExpand("payment");

                var payment = await paymentService.GetAsync(external_id, options);

                if (!string.IsNullOrEmpty(payment.Payment?.PaymentIntentId))
                {
                    var transaction_response = await this.GetTransaction(payment.Payment.PaymentIntentId);
                    if (transaction_response.Success && transaction_response.Data != null)
                        response.Data = transaction_response.Data;
                }
                else
                {
                    response.Data = new PaymentProviderTransactionGetDto()
                    {
                        status = payment.Status, 
                        payment_id = payment.Id, 
                        charge_id = payment.Payment?.ChargeId, 
                        auth_code = "",
                        receipt_number = "",
                        amount_received = payment.AmountPaid == null ? 0 : payment.AmountPaid.Value,
                        transaction_method = "",
                        confirmation_code = "",
                        payment_receipt = "",
                        payment_date = payment.Created
                    };
                }
                
            }
            else
            {
                var paymentIntentService = new Stripe.PaymentIntentService();
                var options = new Stripe.PaymentIntentGetOptions();
                options.AddExpand("latest_charge");
                
                var paymentIntent = await paymentIntentService.GetAsync(external_id, options);

                response.Data = new PaymentProviderTransactionGetDto()
                {
                    status = paymentIntent.Status, 
                    payment_id = paymentIntent.Id, 
                    charge_id = paymentIntent.LatestChargeId, 
                    auth_code = paymentIntent.LatestCharge?.AuthorizationCode,
                    receipt_number = paymentIntent.LatestCharge?.ReceiptNumber,
                    amount_received = paymentIntent.AmountReceived,
                    transaction_method = paymentIntent.LatestCharge?.PaymentMethod,
                    confirmation_code = paymentIntent.LatestCharge?.BalanceTransactionId,
                    payment_receipt = paymentIntent.LatestCharge?.ReceiptUrl,
                    payment_date = paymentIntent.Created
                };
            }
        }
        catch(Exception ex)
        {
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public Task<Response<PaymentTransaction>> ReverseTransaction(string external_id)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<SavedPaymentMethodsDto>> GetSavedPaymentMethods(string external_id)
    {
        Response<SavedPaymentMethodsDto> response = new Response<SavedPaymentMethodsDto>();
        response.Data = new SavedPaymentMethodsDto
        {
            credit_cards = new List<SavedCreditCardDto>(),
            banks = new List<SavedBankDto>()
        };
        

        try
        {
            // Verify customer exists first
            var customerService = new Stripe.CustomerService();
            var customer = await customerService.GetAsync(external_id);
            
            if (customer == null)
                throw new Exception($"Customer {external_id} not found in Stripe.");

            var service = new Stripe.PaymentMethodService();

            var cardOptions = new Stripe.PaymentMethodListOptions
            {
                Customer = external_id,
                Type = "card",
                Limit = 100
            };

            var bankOptions = new Stripe.PaymentMethodListOptions
            {
                Customer = external_id,
                Type = "us_bank_account",
                Limit = 100
            };

            var cards = await service.ListAsync(cardOptions);
            var banks = await service.ListAsync(bankOptions);

            if (cards != null)
            {
                foreach (var card in cards)
                {
                    response.Data.credit_cards.Add(new SavedCreditCardDto
                    {
                        id = card.Id,
                        brand = card.Card.Brand,
                        last4 = card.Card.Last4,
                        exp_month = card.Card.ExpMonth.ToString(),
                        exp_year = card.Card.ExpYear.ToString()
                    });
                }
            }

            if (banks != null)
            {
                foreach (var bank in banks)
                {
                    response.Data.banks.Add(new SavedBankDto
                    {
                        id = bank.Id,
                        bank = bank.UsBankAccount.BankName,
                        last4 = bank.UsBankAccount.Last4
                    });
                }
            }
        }
        catch (Exception ex)
        {
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<Response<PaymentProviderInvoiceDto>> CreateInvoice(ARInvoiceHeader ar_invoice_header, List<ARInvoiceLine> lines, Customer customer)
    {
        Response<PaymentProviderInvoiceDto> response = new Response<PaymentProviderInvoiceDto>();

        try
        {
            int days = (ar_invoice_header.invoice_due_date.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days;

            var invoiceService = new Stripe.InvoiceService();
            var invoice = await invoiceService.CreateAsync(new Stripe.InvoiceCreateOptions
            {
                Customer = customer.payment_external_id,
                CollectionMethod = "send_invoice",
                DaysUntilDue = days,
                Metadata = new Dictionary<string, string>
                {
                    { "Invoice #", ar_invoice_header.invoice_number.ToString() },
                    { "Customer #", customer.customer_number.ToString() }
                }
            });

            var invoiceItemService = new Stripe.InvoiceItemService();

            foreach(var line in lines)
            {
                await invoiceItemService.CreateAsync(new Stripe.InvoiceItemCreateOptions
                {
                    Customer = customer.payment_external_id,
                    Invoice = invoice.Id,
                    Amount = (long)(line.line_total * 100),
                    Currency = "usd",
                    Description = line.line_description
                });
            }

            response.Data = new PaymentProviderInvoiceDto
            {
                id = invoice.Id,
                invoice_number = invoice.Number,
                invoice_url = invoice.HostedInvoiceUrl,
                amount_due = invoice.AmountDue / 100m,
                status = invoice.Status
            };
        }
        catch(Exception ex)
        {
            response.SetException(ex.Message, ResultCode.Error);
            return response;
        }
        

        return response;
    }

    public async Task<Response<PaymentProviderInvoiceDto>> CompleteInvoice(string invoice_external_id, string payment_external_id)
    {
        Response<PaymentProviderInvoiceDto> response = new Response<PaymentProviderInvoiceDto>();

        try
        {
            var paymentIntentService = new Stripe.PaymentIntentService();
            var payment = await paymentIntentService.GetAsync(payment_external_id);

            var invoiceService = new Stripe.InvoiceService();
            
            var invoice = await invoiceService.AttachPaymentAsync(invoice_external_id, new Stripe.InvoiceAttachPaymentOptions
            {
                PaymentIntent = payment.Id
            });


            response.Data = new PaymentProviderInvoiceDto
            {
                id = invoice.Id,
                invoice_number = invoice.Number,
                invoice_url = invoice.HostedInvoiceUrl,
                amount_due = invoice.AmountDue / 100m,
                status = invoice.Status
            };
        }
        catch(Exception ex)
        {
            response.SetException(ex.Message, ResultCode.Error);
            return response;
        }

        return response;

    }

    public async Task<Response<PaymentProviderInvoiceDto>> GetInvoice(string external_id)
    {
        Response<PaymentProviderInvoiceDto> response = new Response<PaymentProviderInvoiceDto>();

        try
        {
            var invoiceService = new Stripe.InvoiceService();

            var options = new Stripe.InvoiceGetOptions();
            options.AddExpand("payments");

            var invoice = await invoiceService.GetAsync(external_id, options);
            
            var invoice_dto = new PaymentProviderInvoiceDto
            {
                id = invoice.Id,
                invoice_number = invoice.Number,
                invoice_url = invoice.HostedInvoiceUrl,
                amount_due = invoice.AmountDue / 100m,
                status = invoice.Status
            };

            foreach(var payment in invoice.Payments)
            {
                var transaction_response = await this.GetTransaction(payment.Id);

                if(transaction_response.Success && transaction_response.Data != null)
                    invoice_dto.transactions.Add(transaction_response.Data);
            }

            

            response.Data = invoice_dto;
        }
        catch(Exception ex)
        {
            response.SetException(ex.Message, ResultCode.Error);
            return response;
        }

        return response;
    }

    public async Task<Response<PaymentCardGetDto>> CreateCard(Customer customer)
    {
        Response<PaymentCardGetDto> response = new Response<PaymentCardGetDto>();

        try
        {
            var service = new Stripe.SetupIntentService();
            var intent = await service.CreateAsync(new Stripe.SetupIntentCreateOptions
            {
                Customer = customer.payment_external_id,
                PaymentMethodTypes = new List<string> { "card" }
            });

            response.Data = new PaymentCardGetDto
            {
                identifier = intent.Id,
                clientSecret = intent.ClientSecret,
                success = true
            };
        }
        catch(Exception ex)
        {
            response.SetException(ex.Message, ResultCode.Error);
        }
        
        return response;
    }
}