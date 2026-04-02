using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace WebApp1.Models.Db;

public partial class Csi402dbContext : DbContext
{
    public Csi402dbContext()
    {
    }

    public Csi402dbContext(DbContextOptions<Csi402dbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Adminuser> Adminusers { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Labstudent> Labstudents { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductColor> ProductColors { get; set; }

    public virtual DbSet<ProductColorImage> ProductColorImages { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductSize> ProductSizes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=csi402db;user=root;password=root;sslmode=None;allowpublickeyretrieval=True", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.43-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Adminuser>(entity =>
        {
            entity.HasKey(e => e.AdminUserId).HasName("PRIMARY");

            entity.ToTable("adminuser");

            entity.Property(e => e.AdminUserId)
                .HasMaxLength(20)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.EmailAdmin)
                .HasMaxLength(100)
                .HasColumnName("Email_Admin")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.LastnameAdmin)
                .HasMaxLength(100)
                .HasColumnName("Lastname_Admin")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.NameAdmin)
                .HasMaxLength(50)
                .HasColumnName("Name_Admin")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Password)
                .HasMaxLength(80)
                .HasColumnName("PASSWORD")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Support'")
                .HasColumnName("role_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.PhonenumberAdmin)
                .HasMaxLength(20)
                .HasColumnName("Phonenumber_Admin")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PRIMARY");

            entity.ToTable("brands");

            entity.HasIndex(e => e.BrandName, "UX_brands_name").IsUnique();

            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.BrandName)
                .HasMaxLength(100)
                .HasColumnName("brand_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity.ToTable("categories");

            entity.HasIndex(e => e.IsActive, "IX_categories_active");

            entity.HasIndex(e => e.ParentId, "IX_categories_parent");

            entity.HasIndex(e => new { e.ParentId, e.CategoryName }, "UX_categories_parent_name").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(100)
                .HasColumnName("category_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.ParentId)
                .HasComment("NULL = parent category, มีค่า = sub-category")
                .HasColumnName("parent_id");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_categories_parent");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("customer");

            entity.HasIndex(e => e.EmailCustomer, "UX_customer_Email_Customer").IsUnique();

            entity.HasIndex(e => e.Phonenumber, "UX_customer_Phonenumber").IsUnique();

            entity.Property(e => e.EmailCustomer)
                .HasMaxLength(100)
                .HasColumnName("Email_Customer");
            entity.Property(e => e.LastnameCustomer)
                .HasMaxLength(100)
                .HasColumnName("Lastname_Customer");
            entity.Property(e => e.NameCustomer)
                .HasMaxLength(50)
                .HasColumnName("Name_Customer");
            entity.Property(e => e.Password)
                .HasMaxLength(80)
                .HasColumnName("PASSWORD");
            entity.Property(e => e.Phonenumber).HasMaxLength(20);
            entity.Property(e => e.Utimestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("UTIMESTAMP");
        });

        modelBuilder.Entity<Labstudent>(entity =>
        {
            entity.HasKey(e => e.StdId).HasName("PRIMARY");

            entity.ToTable("labstudent");

            entity.Property(e => e.StdId)
                .HasMaxLength(10)
                .HasColumnName("StdID")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.StdLastName)
                .HasMaxLength(100)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.StdName)
                .HasMaxLength(50)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.StdPassword)
                .HasMaxLength(30)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PRIMARY");

            entity.ToTable("products");

            entity.HasIndex(e => e.BrandId, "IX_products_brand");

            entity.HasIndex(e => e.CategoryId, "IX_products_category");

            entity.HasIndex(e => new { e.Gender, e.Status }, "IX_products_gender_status");

            entity.HasIndex(e => new { e.ShowOnHomepage, e.Status }, "IX_products_homepage");

            entity.HasIndex(e => e.Status, "IX_products_status");

            entity.HasIndex(e => e.Sku, "UX_products_sku").IsUnique();

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CostPrice)
                .HasPrecision(10, 2)
                .HasColumnName("cost_price");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(5, 2)
                .HasComputedColumnSql("case when ((`regular_price` > 0) and (`sale_price` is not null) and (`sale_price` < `regular_price`)) then round((((`regular_price` - `sale_price`) / `regular_price`) * 100),2) else NULL end", true)
                .HasColumnName("discount_percent");
            entity.Property(e => e.FullDescription)
                .HasColumnType("text")
                .HasColumnName("full_description");
            entity.Property(e => e.Gender)
                .HasColumnType("enum('Men','Women','Unisex')")
                .HasColumnName("gender");
            entity.Property(e => e.HighlightFeatures)
                .HasColumnType("text")
                .HasColumnName("highlight_features");
            entity.Property(e => e.ProductName)
                .HasMaxLength(200)
                .HasColumnName("product_name");
            entity.Property(e => e.RegularPrice)
                .HasPrecision(10, 2)
                .HasColumnName("regular_price");
            entity.Property(e => e.SalePrice)
                .HasPrecision(10, 2)
                .HasColumnName("sale_price");
            entity.Property(e => e.ShortDescription)
                .HasMaxLength(500)
                .HasColumnName("short_description");
            entity.Property(e => e.ShowOnHomepage).HasColumnName("show_on_homepage");
            entity.Property(e => e.Sku)
                .HasMaxLength(100)
                .HasColumnName("sku");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Draft'")
                .HasColumnType("enum('Draft','Active','Inactive')")
                .HasColumnName("status");
            entity.Property(e => e.TechnicalDetails)
                .HasColumnType("text")
                .HasColumnName("technical_details");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_products_brand");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_products_category");
        });

        modelBuilder.Entity<ProductColor>(entity =>
        {
            entity.HasKey(e => e.ColorId).HasName("PRIMARY");

            entity.ToTable("product_colors");

            entity.HasIndex(e => e.ProductId, "IX_product_colors_product");

            entity.HasIndex(e => new { e.ProductId, e.ColorName }, "UX_product_colors_unique").IsUnique();

            entity.Property(e => e.ColorId).HasColumnName("color_id");
            entity.Property(e => e.ColorName)
                .HasMaxLength(100)
                .HasColumnName("color_name");
            entity.Property(e => e.ProductId).HasColumnName("product_id");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductColors)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_product_colors_product");
        });

        modelBuilder.Entity<ProductColorImage>(entity =>
        {
            entity.HasKey(e => e.ColorImageId).HasName("PRIMARY");

            entity.ToTable("product_color_images");

            entity.HasIndex(e => e.ColorId, "IX_color_images_color");

            entity.Property(e => e.ColorImageId).HasColumnName("color_image_id");
            entity.Property(e => e.ColorId).HasColumnName("color_id");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");

            entity.HasOne(d => d.Color).WithMany(p => p.ProductColorImages)
                .HasForeignKey(d => d.ColorId)
                .HasConstraintName("FK_color_images_color");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PRIMARY");

            entity.ToTable("product_images");

            entity.HasIndex(e => e.ProductId, "IX_product_images_product");

            entity.HasIndex(e => new { e.ProductId, e.ImageType }, "IX_product_images_type");

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.ImageType)
                .HasColumnType("enum('Thumbnail','Gallery')")
                .HasColumnName("image_type");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_product_images_product");
        });

        modelBuilder.Entity<ProductSize>(entity =>
        {
            entity.HasKey(e => e.SizeId).HasName("PRIMARY");

            entity.ToTable("product_sizes");

            entity.HasIndex(e => e.ColorId, "IX_product_sizes_color");

            entity.HasIndex(e => new { e.ColorId, e.StockQuantity }, "IX_product_sizes_stock");

            entity.HasIndex(e => new { e.ColorId, e.SizeType, e.SizeValue }, "UX_product_sizes_unique").IsUnique();

            entity.Property(e => e.SizeId).HasColumnName("size_id");
            entity.Property(e => e.ColorId).HasColumnName("color_id");
            entity.Property(e => e.SizeType)
                .HasColumnType("enum('EU','US')")
                .HasColumnName("size_type");
            entity.Property(e => e.SizeValue)
                .HasMaxLength(20)
                .HasColumnName("size_value");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Active'")
                .HasColumnType("enum('Active','Inactive')")
                .HasColumnName("status");
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity");

            entity.HasOne(d => d.Color).WithMany(p => p.ProductSizes)
                .HasForeignKey(d => d.ColorId)
                .HasConstraintName("FK_product_sizes_color");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PRIMARY");

            entity.ToTable("orders");

            entity.HasIndex(e => e.OrderNumber, "UX_orders_order_number").IsUnique();
            entity.HasIndex(e => e.UserId, "IX_orders_user_id");
            entity.HasIndex(e => e.Status, "IX_orders_status");
            entity.HasIndex(e => e.OrderDate, "IX_orders_order_date");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.OrderNumber).HasMaxLength(20).HasColumnName("order_number");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2).HasColumnName("total_amount");
            entity.Property(e => e.ShippingFee).HasPrecision(18, 2).HasColumnName("shipping_fee");
            entity.Property(e => e.Discount).HasPrecision(18, 2).HasColumnName("discount");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50).HasColumnName("payment_method");
            entity.Property(e => e.PaymentStatus)
                .HasColumnType("enum('Pending','Paid','Failed')")
                .HasDefaultValueSql("'Pending'")
                .HasColumnName("payment_status");
            entity.Property(e => e.Status)
                .HasColumnType("enum('Pending','Processing','Shipped','Delivered','Cancelled')")
                .HasDefaultValueSql("'Pending'")
                .HasColumnName("status");
            entity.Property(e => e.ShippingAddress).HasMaxLength(500).HasColumnName("shipping_address");
            entity.Property(e => e.ShippingMethod).HasMaxLength(100).HasColumnName("shipping_method");
            entity.Property(e => e.TrackingNumber).HasMaxLength(100).HasColumnName("tracking_number");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("order_date");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Customer).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_orders_customer");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PRIMARY");

            entity.ToTable("order_items");

            entity.HasIndex(e => e.OrderId, "IX_order_items_order_id");
            entity.HasIndex(e => e.SizeId, "IX_order_items_size_id");

            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.SizeId).HasColumnName("size_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2).HasColumnName("unit_price");
            entity.Property(e => e.LineTotal)
                .HasPrecision(18, 2)
                .HasComputedColumnSql("`quantity` * `unit_price`", stored: true)
                .HasColumnName("line_total");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_order_items_order");

            entity.HasOne(d => d.ProductSize).WithMany()
                .HasForeignKey(d => d.SizeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_order_items_product_size");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
