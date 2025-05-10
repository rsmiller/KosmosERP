//using System.Data;
//using Microsoft.EntityFrameworkCore;
//using KosmosERP.Models;
//using KosmosERP.Models.Permissions;
//using KosmosERP.Tests.Modules.Shared;
//using KosmosERP.BusinessLayer.Helpers;
//using KosmosERP.BusinessLayer.Modules;
//using KosmosERP.Database.Models;
//using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Create;
//using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Edit;
//using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Delete;
//using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Find;
//using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Dto;
//using KosmosERP.BusinessLayer.MessagePublisher;

//namespace KosmosERP.Tests.Modules;

//public class ProductionOrderModuleTests : BaseTestModule<ProductionOrderModule>, IModuleTest
//{
//    private Address _Address;
//    private Customer _Customer;
//    private Product _Product;

//    [SetUp]
//    public async Task SetupModule()
//    {
//        var the_module = new ProductionOrderModule(base._Context, new MockMessagePublisher(new MessagePublisherSettings()));

//        await base.SetupModule(the_module);
//    }

//    protected override async Task SetupRoles()
//    {
//        var admin_role = CommonDataHelper<Role>.FillCommonFields(new Role()
//        {
//            name = "Module Admin",
//        }, 1);

//        _Context.Roles.Add(admin_role);
//        await _Context.SaveChangesAsync();

//        var user_role = CommonDataHelper<UserRole>.FillCommonFields(new UserRole()
//        {
//            role_id = admin_role.id,
//            user_id = _User.id,
//        }, 1);

//        _Context.UserRoles.Add(user_role);
//        await _Context.SaveChangesAsync();
//    }

//    protected override async Task SetupReadPermissions()
//    {
//        var read_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ProductionOrderPermissions.Read);
//        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

//        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
//        {
//            module_permission_id = read_permission.id,
//            role_id = role.id,
//        }, 1);

//        _Context.RolePermissions.Add(role_module_permission);
//        await _Context.SaveChangesAsync();
//    }

//    protected override async Task SetupCreatePermissions()
//    {
//        var create_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ProductionOrderPermissions.Create);
//        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

//        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
//        {
//            module_permission_id = create_permission.id,
//            role_id = role.id,
//        }, 1);

//        _Context.RolePermissions.Add(role_module_permission);
//        await _Context.SaveChangesAsync();
//    }

//    protected override async Task SetupEditPermissions()
//    {
//        var edit_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ProductionOrderPermissions.Edit);
//        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

//        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
//        {
//            module_permission_id = edit_permission.id,
//            role_id = role.id,
//        }, 1);

//        _Context.RolePermissions.Add(role_module_permission);
//        await _Context.SaveChangesAsync();
//    }

//    protected override async Task SetupDeletePermissions()
//    {
//        var delete_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == ProductionOrderPermissions.Delete);
//        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

//        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
//        {
//            module_permission_id = delete_permission.id,
//            role_id = role.id,
//        }, 1);

//        _Context.RolePermissions.Add(role_module_permission);
//        await _Context.SaveChangesAsync();
//    }

//    protected override async Task SetupData()
//    {
//        //if (_Product != null)
//        //    return;


//        var product = CommonDataHelper<Product>.FillCommonFields(new Product()
//        {
//            category = "Slings",
//            sales_price = 100,
//            list_price = 150,
//            product_class = "Rope",
//            product_name = "100ft Wirerope Sling",
//            internal_description = "This is a cool sling",
//            external_description = "This is a cool sling",
//            identifier1 = "SL-100-C",
//            our_cost = 40,
//            unit_cost = 60,
//            is_taxable = true,
//            is_shippable = true,
//            is_sales_item = true,
//        }, 1);

//        _Context.Products.Add(product);
//        await _Context.SaveChangesAsync();

//        _Product = product;


//        var address = CommonDataHelper<Address>.FillCommonFields(new Address
//        {
//            street_address1 = "11005 Chicken Nugget Lane",
//            street_address2 = "Unit 12",
//            city = "Temple",
//            state = "TX",
//            postal_code = "76251",
//            country = "USA",
//            is_deleted = false,
//        }, 1);

//        _Context.Addresses.Add(address);
//        await _Context.SaveChangesAsync();

//        _Address = address;


//        var customer = CommonDataHelper<Customer>.FillCommonFields(new Customer()
//        {
//            category = "CAT1",
//            fax = "123-123-1234",
//            phone = "234-456-2312",
//            general_email = "vendor@vendor.com",
//            is_deleted = false,
//            customer_name = "Some customer",
//            website = "google.com",
//            tax_rate = 10,
//            is_taxable = true
//        }, 1);

//        _Context.Customers.Add(customer);
//        await _Context.SaveChangesAsync();

//        _Customer = customer;


//        var customer_address = CommonDataHelper<CustomerAddress>.FillCommonFields(new CustomerAddress
//        {
//            customer_id = _Customer.id,
//            address_type_id = CustomerAddressType.Physical,
//            address_id = _Address.id,
//        }, 1);

//        _Context.CustomerAddresses.Add(customer_address);
//        await _Context.SaveChangesAsync();

//        var customer_shipto_address = CommonDataHelper<CustomerAddress>.FillCommonFields(new CustomerAddress
//        {
//            customer_id = _Customer.id,
//            address_type_id = CustomerAddressType.ShipTo,
//            address_id = _Address.id,
//        }, 1);

//        _Context.CustomerAddresses.Add(customer_shipto_address);
//        await _Context.SaveChangesAsync();
//    }

//    [Test]
//    public async Task Get()
//    {
//        var new_result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
//        {
//            calling_user_id = 1,
//            token = _SessionId,
//            ProductionOrder_date = DateOnly.FromDateTime(DateTime.Now),
//            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
//            customer_id = _Customer.id,
//            po_number = "123456",
//            ship_to_address_id = _Address.id,
//            ProductionOrder_type = "R",
//            pay_method_id = 1,
//            shipping_cost = 12,
//            shipping_method_id = 1,
//            ProductionOrder_lines = new List<ProductionOrderLineCreateCommand>() {
//                new ProductionOrderLineCreateCommand()
//                {
//                    quantity = 1,
//                    unit_price = 10,
//                    line_number = 1,
//                    line_description = "Super cool line",
//                    product_id = _Product.id,
//                    attributes = new List<ProductionOrderLineAttributeCreateCommand>()
//                    {
//                        new ProductionOrderLineAttributeCreateCommand()
//                        {
//                            attribute_name = "Length",
//                            attribute_value = "10ft"
//                        },
//                        new ProductionOrderLineAttributeCreateCommand()
//                        {
//                            attribute_name = "Material",
//                            attribute_value = "Plastic"
//                        }
//                    }
//                }
//            }
//        });

//        Assert.IsTrue(new_result.Success);
//        Assert.IsNotNull(new_result.Data);

//        var result = await _Module.GetDto(new_result.Data.id);

//        ValidateMostDtoFields(result);

//        decimal expected_tax = (10 * 1) * 10 + (10 * 1);

//        Assert.That(result.Data.tax == expected_tax);
//    }

//    [Test]
//    public async Task Create()
//    {
//        var result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
//        {
//            calling_user_id = 1,
//            token = _SessionId,
//            ProductionOrder_date = DateOnly.FromDateTime(DateTime.Now),
//            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
//            customer_id = _Customer.id,
//            po_number = "556644",
//            ship_to_address_id = _Address.id,
//            ProductionOrder_type = "R",
//            pay_method_id = 1,
//            shipping_cost = 12,
//            shipping_method_id = 1,
//            ProductionOrder_lines = new List<ProductionOrderLineCreateCommand>() {
//                new ProductionOrderLineCreateCommand()
//                {
//                    quantity = 1,
//                    unit_price = 10,
//                    line_number = 1,
//                    line_description = "Super cool line",
//                    product_id = _Product.id,
//                    attributes = new List<ProductionOrderLineAttributeCreateCommand>()
//                    {
//                        new ProductionOrderLineAttributeCreateCommand()
//                        {
//                            attribute_name = "Length",
//                            attribute_value = "10ft"
//                        },
//                        new ProductionOrderLineAttributeCreateCommand()
//                        {
//                            attribute_name = "Material",
//                            attribute_value = "Plastic"
//                        }
//                    }
//                }
//            }
//        });

//        ValidateMostDtoFields(result);
//    }

//    [Test]
//    public async Task Edit()
//    {
//        var old_result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
//        {
//            calling_user_id = 1,
//            token = _SessionId,
//            ProductionOrder_date = DateOnly.FromDateTime(DateTime.Now),
//            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
//            customer_id = _Customer.id,
//            po_number = "1112323",
//            ship_to_address_id = _Address.id,
//            ProductionOrder_type = "R",
//            pay_method_id = 1,
//            shipping_cost = 12,
//            shipping_method_id = 1,
//            ProductionOrder_lines = new List<ProductionOrderLineCreateCommand>() {
//                new ProductionOrderLineCreateCommand()
//                {
//                    quantity = 1,
//                    unit_price = 10,
//                    line_number = 1,
//                    line_description = "Super cool line",
//                    product_id = _Product.id,
//                    attributes = new List<ProductionOrderLineAttributeCreateCommand>()
//                    {
//                        new ProductionOrderLineAttributeCreateCommand()
//                        {
//                            attribute_name = "Length",
//                            attribute_value = "10ft"
//                        },
//                        new ProductionOrderLineAttributeCreateCommand()
//                        {
//                            attribute_name = "Material",
//                            attribute_value = "Plastic"
//                        }
//                    }
//                }
//            }
//        });

//        var edit_command = new ProductionOrderHeaderEditCommand()
//        {
//            calling_user_id = 1,
//            token = _SessionId,
//            id = old_result.Data.id,
//            po_number = "23232323",
//            ship_to_address_id = _Address.id,
//            ProductionOrder_type = "D",
//            pay_method_id = 2,
//            shipping_cost = 1232,
//            shipping_method_id = 4,
//            ProductionOrder_date = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
//            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(17)),
//        };

//        var result = await _Module.Edit(edit_command);

//        ValidateMostDtoFields(result);

//        Assert.That(result.Data.po_number == edit_command.po_number);
//        Assert.That(result.Data.ProductionOrder_type == edit_command.ProductionOrder_type);
//        Assert.That(result.Data.pay_method_id == edit_command.pay_method_id);
//        Assert.That(result.Data.shipping_cost == edit_command.shipping_cost);
//        Assert.That(result.Data.shipping_method_id == edit_command.shipping_method_id);
//        Assert.That(result.Data.ProductionOrder_date == edit_command.ProductionOrder_date);
//        Assert.That(result.Data.required_date == edit_command.required_date);
//    }

//    [Test]
//    public async Task Delete()
//    {
//        var new_result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
//        {
//            calling_user_id = 1,
//            token = _SessionId,
//            ProductionOrder_date = DateOnly.FromDateTime(DateTime.Now),
//            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
//            customer_id = _Customer.id,
//            po_number = "9677563",
//            ship_to_address_id = _Address.id,
//            ProductionOrder_type = "R",
//            pay_method_id = 1,
//            shipping_cost = 12,
//            shipping_method_id = 1,
//            ProductionOrder_lines = new List<ProductionOrderLineCreateCommand>() {
//                new ProductionOrderLineCreateCommand()
//                {
//                    quantity = 1,
//                    unit_price = 10,
//                    line_number = 1,
//                    line_description = "Super cool line",
//                    product_id = _Product.id,
//                    attributes = new List<ProductionOrderLineAttributeCreateCommand>()
//                    {
//                        new ProductionOrderLineAttributeCreateCommand()
//                        {
//                            attribute_name = "Length",
//                            attribute_value = "10ft"
//                        },
//                        new ProductionOrderLineAttributeCreateCommand()
//                        {
//                            attribute_name = "Material",
//                            attribute_value = "Plastic"
//                        }
//                    }
//                }
//            }
//        });

//        Assert.IsTrue(new_result.Success);
//        Assert.IsNotNull(new_result.Data);

//        var delete_result = await _Module.Delete(new ProductionOrderHeaderDeleteCommand()
//        {
//            calling_user_id = 1,
//            token = _SessionId,
//            id = new_result.Data.id
//        });

//        Assert.IsTrue(delete_result.Success);
//        Assert.IsNotNull(delete_result.Data);
//        Assert.Zero(delete_result.Data.ProductionOrder_lines.Count());
//        Assert.NotNull(delete_result.Data.deleted_by);
//        Assert.NotNull(delete_result.Data.deleted_on);
//        Assert.NotNull(delete_result.Data.deleted_on_string);
//        Assert.NotNull(delete_result.Data.deleted_on_timezone);
//    }

//    [Test]
//    public async Task Find()
//    {
//        var new_result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
//        {
//            calling_user_id = 1,
//            token = _SessionId,
//            ProductionOrder_date = DateOnly.FromDateTime(DateTime.Now),
//            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
//            customer_id = _Customer.id,
//            po_number = "234234443",
//            ship_to_address_id = _Address.id,
//            ProductionOrder_type = "R",
//            pay_method_id = 1,
//            shipping_cost = 12,
//            shipping_method_id = 1,
//            ProductionOrder_lines = new List<ProductionOrderLineCreateCommand>() {
//                new ProductionOrderLineCreateCommand()
//                {
//                    quantity = 1,
//                    unit_price = 10,
//                    line_number = 1,
//                    line_description = "Super cool line",
//                    product_id = _Product.id,
//                    attributes = new List<ProductionOrderLineAttributeCreateCommand>()
//                    {
//                        new ProductionOrderLineAttributeCreateCommand()
//                        {
//                            attribute_name = "Length",
//                            attribute_value = "10ft"
//                        },
//                        new ProductionOrderLineAttributeCreateCommand()
//                        {
//                            attribute_name = "Material",
//                            attribute_value = "Plastic"
//                        }
//                    }
//                }
//            }
//        });

//        Assert.IsTrue(new_result.Success);
//        Assert.IsNotNull(new_result.Data);

//        ValidateMostDtoFields(new_result);

//        var results = await _Module.Find(
//                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
//                        new ProductionOrderHeaderFindCommand() { calling_user_id = 1, token = _SessionId, wildcard = "234234443" });

//        Assert.IsTrue(results.Success);
//        Assert.IsNotNull(results.Data);
//        Assert.NotZero(results.Data.Count());

//        var first_result = results.Data.First();

//        ValidateMostListFields(first_result);
//    }

    
//    [Test]
//    public async Task CreateLine()
//    {
//        var create_result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
//        {
//            calling_user_id = 1,
//            token = _SessionId,
//            ProductionOrder_date = DateOnly.FromDateTime(DateTime.Now),
//            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
//            customer_id = _Customer.id,
//            po_number = "1231242141234",
//            ship_to_address_id = _Address.id,
//            ProductionOrder_type = "R",
//            pay_method_id = 1,
//            shipping_cost = 12,
//            shipping_method_id = 1,
//            ProductionOrder_lines = new List<ProductionOrderLineCreateCommand>() {
//                new ProductionOrderLineCreateCommand()
//                {
//                    quantity = 1,
//                    unit_price = 10,
//                    line_number = 1,
//                    line_description = "Super cool line",
//                    product_id = _Product.id,
//                    attributes = new List<ProductionOrderLineAttributeCreateCommand>()
//                    {
//                        new ProductionOrderLineAttributeCreateCommand()
//                        {
//                            attribute_name = "Length",
//                            attribute_value = "10ft"
//                        },
//                        new ProductionOrderLineAttributeCreateCommand()
//                        {
//                            attribute_name = "Material",
//                            attribute_value = "Plastic"
//                        }
//                    }
//                }
//            }
//        });

//        Assert.IsTrue(create_result.Success);
//        Assert.IsNotNull(create_result.Data);
//        Assert.NotZero(create_result.Data.ProductionOrder_lines.Count());
//        Assert.NotZero(create_result.Data.ProductionOrder_lines[0].attributes.Count());

//        var create_command = new ProductionOrderLineCreateCommand()
//        {
//            calling_user_id = 1,
//            token = _SessionId,
//            ProductionOrder_header_id = create_result.Data.id,
//            quantity = 111,
//            unit_price = 111,
//            line_number = 2,
//            line_description = "AnotherSuper cool line",
//            product_id = _Product.id,
//            attributes = new List<ProductionOrderLineAttributeCreateCommand>()
//                {
//                    new ProductionOrderLineAttributeCreateCommand()
//                    {
//                        attribute_name = "Length",
//                        attribute_value = "10ft"
//                    },
//                    new ProductionOrderLineAttributeCreateCommand()
//                    {
//                        attribute_name = "Material",
//                        attribute_value = "Plastic"
//                    }
//                }
//        };

//        var create_line_response = await _Module.CreateLine(create_command);


//        Assert.IsTrue(create_line_response.Success);
//        Assert.IsNotNull(create_line_response.Data);
//        Assert.That(create_line_response.Data.line_number == create_command.line_number);
//        Assert.That(create_line_response.Data.quantity == create_command.quantity);
//        Assert.That(create_line_response.Data.unit_price == create_command.unit_price);
//    }

//    [Test]
//    public async Task EditLine()
//    {
//        var create_result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
//        {
//            calling_user_id = 1,
//            token = _SessionId,
//            ProductionOrder_date = DateOnly.FromDateTime(DateTime.Now),
//            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
//            customer_id = _Customer.id,
//            po_number = "3478955588",
//            ship_to_address_id = _Address.id,
//            ProductionOrder_type = "R",
//            pay_method_id = 1,
//            shipping_cost = 12,
//            shipping_method_id = 1,
//            ProductionOrder_lines = new List<ProductionOrderLineCreateCommand>() {
//                new ProductionOrderLineCreateCommand()
//                {
//                    quantity = 1,
//                    unit_price = 10,
//                    line_number = 1,
//                    line_description = "Super cool line",
//                    product_id = _Product.id,
//                    attributes = new List<ProductionOrderLineAttributeCreateCommand>()
//                    {
//                        new ProductionOrderLineAttributeCreateCommand()
//                        {
//                            attribute_name = "Length",
//                            attribute_value = "10ft"
//                        },
//                        new ProductionOrderLineAttributeCreateCommand()
//                        {
//                            attribute_name = "Material",
//                            attribute_value = "Plastic"
//                        }
//                    }
//                }
//            }
//        });

//        Assert.IsTrue(create_result.Success);
//        Assert.IsNotNull(create_result.Data);
//        Assert.NotZero(create_result.Data.ProductionOrder_lines.Count());
//        Assert.NotZero(create_result.Data.ProductionOrder_lines[0].attributes.Count());

//        var edit_command = new ProductionOrderLineEditCommand()
//        {
//            calling_user_id = 1,
//            token = _SessionId,
//            id = create_result.Data.ProductionOrder_lines[0].id,
//            quantity = 111,
//            unit_price = 111,
//            line_number = 34,
//            line_description = "AnotherSuper cool lineasdasdasd",
//            product_id = _Product.id,
//            attributes = new List<ProductionOrderLineAttributeEditCommand>()
//                {
//                    new ProductionOrderLineAttributeEditCommand()
//                    {
//                        attribute_name = "Adding",
//                        attribute_value = "1231231"
//                    },
//                    new ProductionOrderLineAttributeEditCommand()
//                    {
//                        attribute_name = "Material",
//                        attribute_value = "Plastic"
//                    }
//                }
//        };

//        var edit_line_response = await _Module.EditLine(edit_command);


//        Assert.IsTrue(edit_line_response.Success);
//        Assert.IsNotNull(edit_line_response.Data);
//        Assert.That(edit_line_response.Data.line_number == edit_command.line_number);
//        Assert.That(edit_line_response.Data.quantity == edit_command.quantity);
//        Assert.That(edit_line_response.Data.unit_price == edit_command.unit_price);
//        Assert.That(edit_line_response.Data.line_number == edit_command.line_number);
//        Assert.That(edit_line_response.Data.line_description == edit_command.line_description);
//    }


//    [Test]
//    public async Task DeleteLine()
//    {
//        var create_result = await _Module.Create(new ProductionOrderHeaderCreateCommand()
//        {
//            calling_user_id = 1,
//            token = _SessionId,
//            ProductionOrder_date = DateOnly.FromDateTime(DateTime.Now),
//            required_date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
//            customer_id = _Customer.id,
//            po_number = "1231243550",
//            ship_to_address_id = _Address.id,
//            ProductionOrder_type = "R",
//            pay_method_id = 1,
//            shipping_cost = 12,
//            shipping_method_id = 1,
//            ProductionOrder_lines = new List<ProductionOrderLineCreateCommand>() {
//                new ProductionOrderLineCreateCommand()
//                {
//                    quantity = 1,
//                    unit_price = 10,
//                    line_number = 1,
//                    line_description = "Super cool line",
//                    product_id = _Product.id,
//                    attributes = new List<ProductionOrderLineAttributeCreateCommand>()
//                    {
//                        new ProductionOrderLineAttributeCreateCommand()
//                        {
//                            attribute_name = "Length",
//                            attribute_value = "10ft"
//                        },
//                        new ProductionOrderLineAttributeCreateCommand()
//                        {
//                            attribute_name = "Material",
//                            attribute_value = "Plastic"
//                        }
//                    }
//                }
//            }
//        });

//        Assert.IsTrue(create_result.Success);
//        Assert.IsNotNull(create_result.Data);
//        Assert.NotZero(create_result.Data.ProductionOrder_lines.Count());

//        var response = await _Module.DeleteLine(new ProductionOrderLineDeleteCommand()
//        {
//            calling_user_id = 1,
//            token = _SessionId,
//            id = create_result.Data.ProductionOrder_lines[0].id,
//        });


//        Assert.IsTrue(response.Success);
//        Assert.IsNotNull(response.Data);
//        Assert.NotNull(response.Data.deleted_by);
//        Assert.NotNull(response.Data.deleted_on);
//        Assert.NotNull(response.Data.deleted_on_string);
//        Assert.NotNull(response.Data.deleted_on_timezone);
//    }


//    private void ValidateMostDtoFields(Response<ProductionOrderHeaderDto> result)
//    {
//        Assert.IsTrue(result.Success);
//        Assert.IsNotNull(result.Data);
//        Assert.NotZero(result.Data.id);
//        Assert.IsNotEmpty(result.Data.guid);
//        Assert.NotZero(result.Data.ProductionOrder_number);
//        Assert.NotZero(result.Data.customer_id);
//        Assert.NotNull(result.Data.ProductionOrder_type);
//        Assert.NotNull(result.Data.pay_method_id);
//        Assert.NotZero(result.Data.shipping_method_id);
//        Assert.NotZero(result.Data.ship_to_address_id);
//        Assert.NotNull(result.Data.po_number);
//        Assert.NotZero(result.Data.price);
//        Assert.NotZero(result.Data.revision_number);
//        Assert.NotZero(result.Data.tax);
//        Assert.NotZero(result.Data.created_by);
//        Assert.NotNull(result.Data.created_on);
//        Assert.NotNull(result.Data.created_on_string);
//        Assert.NotNull(result.Data.created_on_timezone);
//        Assert.NotNull(result.Data.updated_by);
//        Assert.NotNull(result.Data.updated_on);
//        Assert.NotNull(result.Data.updated_on_string);
//        Assert.NotNull(result.Data.updated_on_timezone);
//    }

//    private void ValidateMostListFields(ProductionOrderHeaderListDto result)
//    {
//        Assert.NotZero(result.id);
//        Assert.IsNotEmpty(result.guid);
//        Assert.NotZero(result.ProductionOrder_number);
//        Assert.NotZero(result.customer_id);
//        Assert.NotNull(result.ProductionOrder_type);
//        Assert.NotNull(result.pay_method_id);
//        Assert.NotZero(result.shipping_method_id);
//        Assert.NotZero(result.ship_to_address_id);
//        Assert.NotNull(result.po_number);
//        Assert.NotZero(result.price);
//        Assert.NotZero(result.revision_number);
//        Assert.NotZero(result.tax);
//        Assert.NotNull(result.created_on);
//        Assert.NotNull(result.created_on_string);
//        Assert.NotNull(result.created_on_timezone);
//        Assert.NotNull(result.updated_by);
//        Assert.NotNull(result.updated_on);
//        Assert.NotNull(result.updated_on_string);
//        Assert.NotNull(result.updated_on_timezone);
//    }
//}