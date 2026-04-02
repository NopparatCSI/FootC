using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WebApp1.Models.Db;
using WebApp1.Security;
using WebApp1.ViewModels;

namespace WebApp1.Controllers
{
    [Authorize(Policy = AdminPolicies.ManageOrders)]
    public class OrderController : Controller
    {
        private readonly Csi402dbContext _context;
        private static readonly string[] OrderStatuses = ["Pending", "Processing", "Shipped", "Delivered", "Refunded", "Cancelled"];
        private static readonly string[] PaymentStatuses = ["Pending", "Paid", "Failed", "Refunded"];

        public OrderController(Csi402dbContext context)
        {
            _context = context;
        }

        // GET /Order/OrdersList
        public IActionResult OrdersList()
        {
            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrdersListItemViewModel
                {
                    OrderId       = o.OrderId,
                    OrderNumber   = o.OrderNumber,
                    CustomerName  = (o.Customer!.NameCustomer ?? "") + " " + (o.Customer.LastnameCustomer ?? ""),
                    CustomerEmail = o.Customer.EmailCustomer ?? "",
                    ItemCount     = o.OrderItems.Sum(i => i.Quantity),
                    TotalAmount   = o.TotalAmount,
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,
                    Status        = o.Status,
                    ShippingMethod = o.ShippingMethod ?? "Standard",
                    TrackingNumber = o.TrackingNumber ?? "",
                    OrderDate     = o.OrderDate,
                    UpdatedAt     = o.UpdatedAt
                })
                .ToList();

            var vm = new OrdersListViewModel { Orders = orders };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdvanceStatus(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return NotFound();

            order.Status = GetNextOrderStatus(order.Status);
            order.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            TempData["OrderActionMessage"] = $"Order {order.OrderNumber}: status updated to {order.Status}.";
            return RedirectToAction(nameof(OrdersList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CyclePaymentStatus(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return NotFound();

            order.PaymentStatus = GetNextPaymentStatus(order.PaymentStatus);
            order.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            TempData["OrderActionMessage"] = $"Order {order.OrderNumber}: payment updated to {order.PaymentStatus}.";
            return RedirectToAction(nameof(OrdersList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateTracking(int orderId, string? shippingMethod, string? trackingNumber)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return NotFound();

            order.ShippingMethod = string.IsNullOrWhiteSpace(shippingMethod) ? null : shippingMethod.Trim();
            order.TrackingNumber = string.IsNullOrWhiteSpace(trackingNumber) ? null : trackingNumber.Trim();
            order.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            TempData["OrderActionMessage"] = $"Order {order.OrderNumber}: tracking details updated.";
            return RedirectToAction(nameof(OrdersList));
        }

        [HttpGet]
        public IActionResult PrintInvoice(int id)
        {
            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize!)
                        .ThenInclude(ps => ps.Color)
                            .ThenInclude(pc => pc.Product)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null) return NotFound();

            var sb = new StringBuilder();
            sb.Append("<!doctype html><html><head><meta charset='utf-8'><title>Invoice ")
                .Append(order.OrderNumber)
                .Append("</title><style>body{font-family:Segoe UI,Arial,sans-serif;padding:24px;color:#111}table{width:100%;border-collapse:collapse;margin-top:12px}th,td{border-bottom:1px solid #ddd;padding:8px;text-align:left}.r{text-align:right}.total{font-weight:700;font-size:18px;margin-top:14px}</style></head><body>")
                .Append("<h2>Invoice ")
                .Append(order.OrderNumber)
                .Append("</h2>")
                .Append("<p>Date: ")
                .Append(order.OrderDate.ToString("dd MMM yyyy HH:mm"))
                .Append("<br>Customer: ")
                .Append((order.Customer?.NameCustomer ?? "") + " " + (order.Customer?.LastnameCustomer ?? ""))
                .Append("<br>Email: ")
                .Append(order.Customer?.EmailCustomer ?? "-")
                .Append("</p>")
                .Append("<table><thead><tr><th>Product</th><th>Qty</th><th class='r'>Unit</th><th class='r'>Total</th></tr></thead><tbody>");

            foreach (var item in order.OrderItems)
            {
                var productName = item.ProductSize?.Color?.Product?.ProductName ?? "-";
                var lineTotal = item.Quantity * item.UnitPrice;
                sb.Append("<tr><td>")
                    .Append(productName)
                    .Append("</td><td>")
                    .Append(item.Quantity)
                    .Append("</td><td class='r'>")
                    .Append(item.UnitPrice.ToString("N2"))
                    .Append("</td><td class='r'>")
                    .Append(lineTotal.ToString("N2"))
                    .Append("</td></tr>");
            }

            var subtotal = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice);
            var grandTotal = subtotal + order.ShippingFee - order.Discount;

            sb.Append("</tbody></table>")
                .Append("<div class='total'>Grand Total: ")
                .Append(grandTotal.ToString("N2"))
                .Append("</div>")
                .Append("<script>window.onload=function(){window.print();}</script></body></html>");

            return Content(sb.ToString(), "text/html");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RefundOrder(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return NotFound();

            // Cannot refund an already refunded or cancelled order
            if (order.Status == "Refunded" || order.Status == "Cancelled")
            {
                TempData["ErrorMessage"] = $"Order {order.OrderNumber} is already {order.Status}.";
                return RedirectToAction(nameof(OrdersList));
            }

            order.Status = "Refunded";
            order.PaymentStatus = "Refunded";
            order.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            TempData["OrderActionMessage"] = $"Order {order.OrderNumber}: refunded successfully.";
            return RedirectToAction(nameof(OrdersList));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DuplicateOrder(int orderId)
        {
            var source = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.OrderId == orderId);
            if (source == null) return NotFound();

            var newOrder = new Order
            {
                OrderNumber = GenerateNextOrderNumber(),
                UserId = source.UserId,
                TotalAmount = source.TotalAmount,
                ShippingFee = source.ShippingFee,
                Discount = source.Discount,
                PaymentMethod = source.PaymentMethod,
                PaymentStatus = "Pending",
                Status = "Pending",
                ShippingAddress = source.ShippingAddress,
                ShippingMethod = source.ShippingMethod,
                TrackingNumber = null,
                OrderDate = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Orders.Add(newOrder);
            _context.SaveChanges();

            foreach (var item in source.OrderItems)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = newOrder.OrderId,
                    SizeId = item.SizeId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            _context.SaveChanges();

            TempData["OrderActionMessage"] = $"Order {source.OrderNumber} duplicated as {newOrder.OrderNumber}.";
            return RedirectToAction(nameof(OrdersList));
        }

        [HttpGet]
        public IActionResult ExportOrdersCsv()
        {
            var rows = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.OrderNumber,
                    CustomerName = (o.Customer!.NameCustomer ?? "") + " " + (o.Customer.LastnameCustomer ?? ""),
                    CustomerEmail = o.Customer.EmailCustomer ?? "",
                    ItemCount = o.OrderItems.Sum(i => i.Quantity),
                    o.TotalAmount,
                    o.PaymentMethod,
                    o.PaymentStatus,
                    o.Status,
                    o.OrderDate
                })
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("OrderNumber,CustomerName,CustomerEmail,ItemCount,TotalAmount,PaymentMethod,PaymentStatus,Status,OrderDate");
            foreach (var r in rows)
            {
                sb.Append('"').Append(r.OrderNumber.Replace("\"", "\"\"")).Append("\",")
                  .Append('"').Append(r.CustomerName.Replace("\"", "\"\"")).Append("\",")
                  .Append('"').Append(r.CustomerEmail.Replace("\"", "\"\"")).Append("\",")
                  .Append(r.ItemCount).Append(',')
                  .Append(r.TotalAmount).Append(',')
                  .Append('"').Append(r.PaymentMethod.Replace("\"", "\"\"")).Append("\",")
                  .Append('"').Append(r.PaymentStatus.Replace("\"", "\"\"")).Append("\",")
                  .Append('"').Append(r.Status.Replace("\"", "\"\"")).Append("\",")
                  .Append('"').Append(r.OrderDate.ToString("yyyy-MM-dd HH:mm:ss")).Append('"')
                  .AppendLine();
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"orders-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
        }

        // GET /Order/OrderStatus?id=1
            public IActionResult OrderStatus(int? id)
        {
                if (!id.HasValue || id.Value <= 0)
                {
                    return View((OrderStatusViewModel?)null);
                }

            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductSize!)
                        .ThenInclude(ps => ps.Color)
                            .ThenInclude(pc => pc.Product)
                                .ThenInclude(p => p.ProductImages)
                    .FirstOrDefault(o => o.OrderId == id.Value);

                if (order == null)
                {
                    ViewBag.LookupOrderId = id.Value;
                    ViewBag.LookupError = $"Order ID {id.Value} was not found.";
                    return View((OrderStatusViewModel?)null);
                }

            var vm = new OrderStatusViewModel
            {
                OrderId         = order.OrderId,
                OrderNumber     = order.OrderNumber,
                Status          = order.Status,
                PaymentStatus   = order.PaymentStatus,
                PaymentMethod   = order.PaymentMethod,
                OrderDate       = order.OrderDate,
                UpdatedAt       = order.UpdatedAt,
                CustomerName    = (order.Customer?.NameCustomer ?? "") + " " + (order.Customer?.LastnameCustomer ?? ""),
                CustomerEmail   = order.Customer?.EmailCustomer ?? "",
                CustomerPhone   = order.Customer?.Phonenumber ?? "-",
                ShippingAddress = order.ShippingAddress ?? "-",
                ShippingMethod  = order.ShippingMethod ?? "Standard",
                TrackingNumber  = order.TrackingNumber ?? "-",
                ShippingFee     = order.ShippingFee,
                Discount        = order.Discount,
                Items = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductName = oi.ProductSize?.Color?.Product?.ProductName ?? "-",
                    Sku         = oi.ProductSize?.Color?.Product?.Sku ?? "-",
                    Color       = oi.ProductSize?.Color?.ColorName ?? "-",
                    Size        = $"{oi.ProductSize?.SizeType} {oi.ProductSize?.SizeValue}",
                    Quantity    = oi.Quantity,
                    UnitPrice   = oi.UnitPrice,
                    ImageUrl    = oi.ProductSize?.Color?.Product?.ProductImages
                                    .FirstOrDefault(img => img.ImageType == "Thumbnail")?.ImageUrl
                }).ToList(),
                Timeline = BuildTimeline(order.Status, order.OrderDate, order.UpdatedAt)
            };

            return View(vm);
        }

        // ── Timeline builder ─────────────────────────────────────────────────
        private static List<OrderTimelineStep> BuildTimeline(string status, DateTime orderDate, DateTime? updatedAt)
        {
            var steps = new[] { "Pending", "Processing", "Shipped", "Delivered" };
            var icons = new Dictionary<string, string>
            {
                ["Pending"]    = "bi-clock",
                ["Processing"] = "bi-gear",
                ["Shipped"]    = "bi-truck",
                ["Delivered"]  = "bi-check-circle",
            };
            bool cancelled = status == "Cancelled";
            int cur = Array.IndexOf(steps, status);

            var list = steps.Select((s, i) => new OrderTimelineStep
            {
                StepName    = s,
                Icon        = icons[s],
                IsCompleted = !cancelled && i < cur,
                IsCurrent   = !cancelled && i == cur,
                IsCancelled = cancelled,
                CompletedAt = !cancelled && i <= cur
                    ? (i == 0 ? orderDate : updatedAt)
                    : null
            }).ToList();

            if (cancelled)
                list.Add(new OrderTimelineStep
                {
                    StepName    = "Cancelled",
                    Icon        = "bi-x-circle",
                    IsCurrent   = true,
                    IsCancelled = true,
                    CompletedAt = updatedAt ?? orderDate
                });

            return list;
        }

        private static string GetNextOrderStatus(string current)
        {
            var idx = Array.IndexOf(OrderStatuses, current);
            if (idx < 0 || idx == OrderStatuses.Length - 1)
            {
                return OrderStatuses[0];
            }

            return OrderStatuses[idx + 1];
        }

        private static string GetNextPaymentStatus(string current)
        {
            var idx = Array.IndexOf(PaymentStatuses, current);
            if (idx < 0 || idx == PaymentStatuses.Length - 1)
            {
                return PaymentStatuses[0];
            }

            return PaymentStatuses[idx + 1];
        }

        private string GenerateNextOrderNumber()
        {
            var seed = DateTime.Now.ToString("yyyyMMddHHmmss");
            for (var i = 0; i < 99; i++)
            {
                var candidate = i == 0 ? $"ORD-{seed}" : $"ORD-{seed}{i:00}";
                if (!_context.Orders.Any(o => o.OrderNumber == candidate))
                {
                    return candidate;
                }
            }

            return $"ORD-{DateTime.Now:yyyyMMddHHmmssff}";
        }
    }
}
