# Product.cshtml — Line by line explanation

ไฟล์อ้างอิง: `Views/Products/Product.cshtml`

เอกสารนี้อธิบายทุกบรรทัด (L001–L236) ของหน้า Product แบบ mock/customer view

---

## 1) Full Source Code (โค้ดเต็ม)

```cshtml
@model WebApp1.ViewModels.ProductCustomerViewModel

@{
  Layout = "_CustomerNavbar";
  ViewData["Title"] = Model.Name;

  var hasSale = Model.SalePrice.HasValue && Model.SalePrice.Value > 0 && Model.SalePrice.Value < Model.RegularPrice;
  var currentPrice = hasSale ? Model.SalePrice!.Value : Model.RegularPrice;
  var oldPrice = hasSale ? Model.RegularPrice : (decimal?)null;
  var discount = Model.DiscountPercent;

  var thumbnail = string.IsNullOrWhiteSpace(Model.ThumbnailUrl)
    ? "/images/Running-Shoe-2.jpeg"
    : Model.ThumbnailUrl;

  var allImages = new List<string> { thumbnail };
  if (Model.GalleryUrls is { Count: > 0 })
  {
    allImages.AddRange(Model.GalleryUrls);
  }
  else
  {
    allImages.Add("/images/Untitled-design-5-2.jpg");
    allImages.Add("/images/images.jpg");
  }

  var euSizes = new[] { "38", "38.5", "39", "40", "40.5", "41", "42", "42.5", "43", "44", "44.5", "45" };
  var usSizes = new[] { "6", "6.5", "7", "7.5", "8", "8.5", "9", "9.5", "10", "10.5", "11", "11.5" };
}

@section Styles {
  <link rel="stylesheet" href="~/css/CustomerProduct.css" asp-append-version="true" />
}

<div class="cp-page">

  <!-- Breadcrumb -->
  <ul class="cp-breadcrumb">
    <li><a href="/">Home</a></li>
    <li><a href="#">@Model.Category</a></li>
    <li><a href="#">@Model.Gender</a></li>
    <li>@Model.Name</li>
  </ul>

  <!-- ==================== HERO ==================== -->
  <div class="cp-hero">

    <!-- LEFT: images -->
    <div class="cp-images">
      <div class="cp-thumbs">
        @for (int i = 0; i < allImages.Count; i++)
        {
          <img class="cp-thumb @(i == 0 ? "active" : "")"
             src="@allImages[i]" alt="thumb @(i+1)"
             onclick="setMainImage(this)" />
        }
      </div>
      <div class="cp-main-img-wrap">
        <img class="cp-main-img" id="mainImage" src="@allImages[0]" alt="@Model.Name" />
        @if (hasSale && discount.HasValue && discount.Value > 0)
        {
          <span class="cp-badge-sale">-@discount.Value.ToString("0.##")%</span>
        }
      </div>
    </div>

    <!-- RIGHT: info -->
    <div class="cp-info">
      <p class="cp-category-label">@Model.Brand &nbsp;·&nbsp; @Model.Category</p>
      <h1 class="cp-product-name">@Model.Name</h1>

      <p style="margin:0.4rem 0 0; font-size:0.88rem; color:#707072;">@Model.Gender's Shoes</p>

      <div class="cp-price-block">
        <span class="cp-price-now">฿@currentPrice.ToString("N0")</span>
        @if (oldPrice.HasValue)
        {
          <span class="cp-price-was">฿@oldPrice.Value.ToString("N0")</span>
        }
        @if (hasSale && discount.HasValue && discount.Value > 0)
        {
          <span class="cp-price-off">@discount.Value.ToString("0.##")% off</span>
        }
      </div>

      <!-- Color swatches -->
      @if (Model.ColorNames is { Count: > 0 })
      {
        <div class="cp-color-section">
          <div class="cp-label">Color — <span id="selectedColor">@Model.ColorNames[0]</span></div>
          <div class="cp-swatches">
            @foreach (var color in Model.ColorNames)
            {
              <button type="button"
                  class="cp-swatch @(color == Model.ColorNames[0] ? "active" : "")"
                  onclick="pickColor(this, '@color')">
                <span class="cp-swatch-label">@color</span>
              </button>
            }
          </div>
        </div>
      }

      <!-- Size grid -->
      <div class="cp-size-section">
        <div class="cp-size-header">
          <div class="cp-label">Select Size</div>
          <span class="cp-size-guide">Size Guide</span>
        </div>

        <div class="cp-size-type-tabs">
          <button class="cp-size-type-tab active" onclick="switchSizeType(this,'EU')">EU</button>
          <button class="cp-size-type-tab" onclick="switchSizeType(this,'US')">US</button>
        </div>

        <div class="cp-size-grid" id="sizeGrid">
          @foreach (var s in euSizes)
          {
            <button type="button" class="cp-size-btn" onclick="pickSize(this)">@s</button>
          }
        </div>
      </div>

      <!-- Quantity -->
      <div class="cp-qty-section">
        <div class="cp-label" style="margin-bottom:0;">Quantity</div>
        <button type="button" class="cp-qty-btn" onclick="changeQty(-1)">−</button>
        <input type="text" class="cp-qty-value" id="qtyValue" value="1" readonly />
        <button type="button" class="cp-qty-btn" onclick="changeQty(1)">+</button>
      </div>

      <!-- CTA -->
      <div class="cp-actions">
        <button type="button" class="cp-btn-bag">Add to Bag</button>
        <button type="button" class="cp-btn-fav">
          <i class="bi bi-heart"></i> Favorite
        </button>
      </div>

      <div class="cp-stock-note">
        <span class="cp-stock-dot"></span> In stock — Free shipping on orders over ฿2,500
      </div>

      <!-- Short description (inline) -->
      <div style="margin-top:1.2rem; font-size:0.9rem; line-height:1.6; color:#333;">
        @Model.ShortDescription
      </div>
    </div>

  </div>
  <!-- /hero -->

  <!-- ==================== DETAIL CARDS ==================== -->
  <div class="cp-details">

    @if (!string.IsNullOrWhiteSpace(Model.FullDescription))
    {
      <div class="cp-detail-card">
        <h4 class="cp-detail-title">Description</h4>
        <p>@Model.FullDescription</p>
      </div>
    }

    @if (!string.IsNullOrWhiteSpace(Model.HighlightFeaturesHtml))
    {
      <div class="cp-detail-card">
        <h4 class="cp-detail-title">Highlights</h4>
        @Html.Raw(Model.HighlightFeaturesHtml)
      </div>
    }

    @if (!string.IsNullOrWhiteSpace(Model.TechnicalDetails))
    {
      <div class="cp-detail-card">
        <h4 class="cp-detail-title">Technical Details</h4>
        <div class="cp-specs">
          @foreach (var item in (Model.TechnicalDetails ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries))
          {
            <span class="cp-spec-chip">@item.Trim()</span>
          }
        </div>
      </div>
    }

  </div>
</div>

@section Scripts {
<script>
(function () {
  // ---------- thumbnail → main image ----------
  window.setMainImage = function (thumb) {
    document.getElementById('mainImage').src = thumb.src;
    document.querySelectorAll('.cp-thumb').forEach(t => t.classList.remove('active'));
    thumb.classList.add('active');
  };

  // ---------- color swatch ----------
  window.pickColor = function (btn, name) {
    document.querySelectorAll('.cp-swatch').forEach(s => s.classList.remove('active'));
    btn.classList.add('active');
    document.getElementById('selectedColor').textContent = name;
  };

  // ---------- size type tabs ----------
  const euSizes = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(euSizes));
  const usSizes = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(usSizes));

  window.switchSizeType = function (tab, type) {
    document.querySelectorAll('.cp-size-type-tab').forEach(t => t.classList.remove('active'));
    tab.classList.add('active');

    const grid = document.getElementById('sizeGrid');
    const sizes = type === 'US' ? usSizes : euSizes;
    grid.innerHTML = sizes.map(s =>
      '<button type="button" class="cp-size-btn" onclick="pickSize(this)">' + s + '</button>'
    ).join('');
  };

  // ---------- size pick ----------
  window.pickSize = function (btn) {
    if (btn.classList.contains('disabled')) return;
    document.querySelectorAll('.cp-size-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
  };

  // ---------- quantity ----------
  window.changeQty = function (d) {
    const el = document.getElementById('qtyValue');
    let v = parseInt(el.value) || 1;
    v = Math.max(1, v + d);
    el.value = v;
  };
})();
</script>
}
```

---

## 2) Annotated Source (อธิบายแทรกในโค้ด)

> ส่วนนี้คือโค้ดเดียวกับหน้า Product แต่เพิ่มคอมเมนต์อธิบายแทรกไว้ในบล็อกเลย

```cshtml
@model WebApp1.ViewModels.ProductCustomerViewModel

@{
  // ใช้ layout ฝั่งลูกค้า และตั้ง title ของหน้า
  Layout = "_CustomerNavbar";
  ViewData["Title"] = Model.Name;

  // กลุ่มคำนวณราคา: เช็คว่ามีส่วนลดจริงไหม
  var hasSale = Model.SalePrice.HasValue && Model.SalePrice.Value > 0 && Model.SalePrice.Value < Model.RegularPrice;
  var currentPrice = hasSale ? Model.SalePrice!.Value : Model.RegularPrice;
  var oldPrice = hasSale ? Model.RegularPrice : (decimal?)null;
  var discount = Model.DiscountPercent;

  // รูปหลัก: ถ้าไม่มีใน model ให้ใช้ fallback
  var thumbnail = string.IsNullOrWhiteSpace(Model.ThumbnailUrl)
    ? "/images/Running-Shoe-2.jpeg"
    : Model.ThumbnailUrl;

  // รวมรูปทั้งหมดไว้ list เดียว (thumbnail + gallery)
  var allImages = new List<string> { thumbnail };
  if (Model.GalleryUrls is { Count: > 0 })
  {
    allImages.AddRange(Model.GalleryUrls);
  }
  else
  {
    // ถ้าไม่มี gallery จาก model ให้ใช้รูปสำรอง
    allImages.Add("/images/Untitled-design-5-2.jpg");
    allImages.Add("/images/images.jpg");
  }

  // เตรียมชุดไซส์สำหรับสลับ EU/US ใน JavaScript
  var euSizes = new[] { "38", "38.5", "39", "40", "40.5", "41", "42", "42.5", "43", "44", "44.5", "45" };
  var usSizes = new[] { "6", "6.5", "7", "7.5", "8", "8.5", "9", "9.5", "10", "10.5", "11", "11.5" };
}

@section Styles {
  <!-- โหลด CSS เฉพาะหน้า product -->
  <link rel="stylesheet" href="~/css/CustomerProduct.css" asp-append-version="true" />
}

<div class="cp-page">
  <!-- breadcrumb นำทาง -->
  <ul class="cp-breadcrumb">
    <li><a href="/">Home</a></li>
    <li><a href="#">@Model.Category</a></li>
    <li><a href="#">@Model.Gender</a></li>
    <li>@Model.Name</li>
  </ul>

  <div class="cp-hero">
    <!-- ซ้าย: รูปสินค้า -->
    <div class="cp-images">
      <div class="cp-thumbs">
        @for (int i = 0; i < allImages.Count; i++)
        {
          <!-- รูปแรก active เริ่มต้น, คลิกเพื่อเปลี่ยนรูปหลัก -->
          <img class="cp-thumb @(i == 0 ? "active" : "")"
             src="@allImages[i]" alt="thumb @(i+1)"
             onclick="setMainImage(this)" />
        }
      </div>

      <div class="cp-main-img-wrap">
        <img class="cp-main-img" id="mainImage" src="@allImages[0]" alt="@Model.Name" />

        @if (hasSale && discount.HasValue && discount.Value > 0)
        {
          <!-- ป้ายเปอร์เซ็นต์ส่วนลดบนรูป -->
          <span class="cp-badge-sale">-@discount.Value.ToString("0.##")%</span>
        }
      </div>
    </div>

    <!-- ขวา: ข้อมูลสินค้า + ตัวเลือก -->
    <div class="cp-info">
      <p class="cp-category-label">@Model.Brand &nbsp;·&nbsp; @Model.Category</p>
      <h1 class="cp-product-name">@Model.Name</h1>

      <p style="margin:0.4rem 0 0; font-size:0.88rem; color:#707072;">@Model.Gender's Shoes</p>

      <div class="cp-price-block">
        <span class="cp-price-now">฿@currentPrice.ToString("N0")</span>

        @if (oldPrice.HasValue)
        {
          <!-- ราคาก่อนลด (ขีดทับ) -->
          <span class="cp-price-was">฿@oldPrice.Value.ToString("N0")</span>
        }

        @if (hasSale && discount.HasValue && discount.Value > 0)
        {
          <span class="cp-price-off">@discount.Value.ToString("0.##")% off</span>
        }
      </div>

      @if (Model.ColorNames is { Count: > 0 })
      {
        <!-- สีสินค้าแบบ swatch -->
        <div class="cp-color-section">
          <div class="cp-label">Color — <span id="selectedColor">@Model.ColorNames[0]</span></div>
          <div class="cp-swatches">
            @foreach (var color in Model.ColorNames)
            {
              <!-- กดแล้วเปลี่ยน active + อัปเดตชื่อสี -->
              <button type="button"
                  class="cp-swatch @(color == Model.ColorNames[0] ? "active" : "")"
                  onclick="pickColor(this, '@color')">
                <span class="cp-swatch-label">@color</span>
              </button>
            }
          </div>
        </div>
      }

      <!-- เลือกไซส์ -->
      <div class="cp-size-section">
        <div class="cp-size-header">
          <div class="cp-label">Select Size</div>
          <span class="cp-size-guide">Size Guide</span>
        </div>

        <div class="cp-size-type-tabs">
          <!-- เริ่มต้นที่ EU -->
          <button class="cp-size-type-tab active" onclick="switchSizeType(this,'EU')">EU</button>
          <button class="cp-size-type-tab" onclick="switchSizeType(this,'US')">US</button>
        </div>

        <div class="cp-size-grid" id="sizeGrid">
          @foreach (var s in euSizes)
          {
            <button type="button" class="cp-size-btn" onclick="pickSize(this)">@s</button>
          }
        </div>
      </div>

      <!-- จำนวนสินค้า -->
      <div class="cp-qty-section">
        <div class="cp-label" style="margin-bottom:0;">Quantity</div>
        <button type="button" class="cp-qty-btn" onclick="changeQty(-1)">−</button>
        <input type="text" class="cp-qty-value" id="qtyValue" value="1" readonly />
        <button type="button" class="cp-qty-btn" onclick="changeQty(1)">+</button>
      </div>

      <!-- ปุ่ม CTA (ยังเป็น mock) -->
      <div class="cp-actions">
        <button type="button" class="cp-btn-bag">Add to Bag</button>
        <button type="button" class="cp-btn-fav">
          <i class="bi bi-heart"></i> Favorite
        </button>
      </div>

      <div class="cp-stock-note">
        <span class="cp-stock-dot"></span> In stock — Free shipping on orders over ฿2,500
      </div>

      <div style="margin-top:1.2rem; font-size:0.9rem; line-height:1.6; color:#333;">
        @Model.ShortDescription
      </div>
    </div>
  </div>

  <!-- รายละเอียดด้านล่าง: แสดงเฉพาะ section ที่มีข้อมูล -->
  <div class="cp-details">
    @if (!string.IsNullOrWhiteSpace(Model.FullDescription))
    {
      <div class="cp-detail-card">
        <h4 class="cp-detail-title">Description</h4>
        <p>@Model.FullDescription</p>
      </div>
    }

    @if (!string.IsNullOrWhiteSpace(Model.HighlightFeaturesHtml))
    {
      <div class="cp-detail-card">
        <h4 class="cp-detail-title">Highlights</h4>
        @Html.Raw(Model.HighlightFeaturesHtml)
      </div>
    }

    @if (!string.IsNullOrWhiteSpace(Model.TechnicalDetails))
    {
      <div class="cp-detail-card">
        <h4 class="cp-detail-title">Technical Details</h4>
        <div class="cp-specs">
          @foreach (var item in (Model.TechnicalDetails ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries))
          {
            <span class="cp-spec-chip">@item.Trim()</span>
          }
        </div>
      </div>
    }
  </div>
</div>

@section Scripts {
<script>
(function () {
  // เปลี่ยนรูปหลักตาม thumbnail ที่กด
  window.setMainImage = function (thumb) {
    document.getElementById('mainImage').src = thumb.src;
    document.querySelectorAll('.cp-thumb').forEach(t => t.classList.remove('active'));
    thumb.classList.add('active');
  };

  // เลือกสี: ทำ active และอัปเดตชื่อสีที่เลือก
  window.pickColor = function (btn, name) {
    document.querySelectorAll('.cp-swatch').forEach(s => s.classList.remove('active'));
    btn.classList.add('active');
    document.getElementById('selectedColor').textContent = name;
  };

  // ส่ง array ไซส์จาก C# ไป JS
  const euSizes = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(euSizes));
  const usSizes = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(usSizes));

  // สลับ EU/US แล้ว render ปุ่มไซส์ใหม่ทั้งกริด
  window.switchSizeType = function (tab, type) {
    document.querySelectorAll('.cp-size-type-tab').forEach(t => t.classList.remove('active'));
    tab.classList.add('active');

    const grid = document.getElementById('sizeGrid');
    const sizes = type === 'US' ? usSizes : euSizes;
    grid.innerHTML = sizes.map(s =>
      '<button type="button" class="cp-size-btn" onclick="pickSize(this)">' + s + '</button>'
    ).join('');
  };

  // เลือกไซส์เดียวให้ active
  window.pickSize = function (btn) {
    if (btn.classList.contains('disabled')) return;
    document.querySelectorAll('.cp-size-btn').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
  };

  // เพิ่ม/ลดจำนวน แต่ไม่ต่ำกว่า 1
  window.changeQty = function (d) {
    const el = document.getElementById('qtyValue');
    let v = parseInt(el.value) || 1;
    v = Math.max(1, v + d);
    el.value = v;
  };
})();
</script>
}
```

---

## 3) Line-by-line (ทุกบรรทัด)

- **L001**: `@model WebApp1.ViewModels.ProductCustomerViewModel` → กำหนดชนิด Model ที่หน้า Razor นี้รับเข้ามา
- **L002**: บรรทัดว่าง → เว้นระยะให้อ่านง่าย
- **L003**: `@{` → เริ่ม C# code block ของ Razor
- **L004**: `Layout = "_CustomerNavbar";` → ใช้ layout ฝั่งลูกค้า (มี navbar/footer ธีมร้าน)
- **L005**: `ViewData["Title"] = Model.Name;` → ตั้ง title ของหน้าเป็นชื่อสินค้า
- **L006**: บรรทัดว่าง
- **L007**: คำนวณ `hasSale` → true เมื่อมีราคาลด และราคาลดน้อยกว่าราคาปกติ
- **L008**: กำหนด `currentPrice` → ถ้ามีลดใช้ `SalePrice` ไม่งั้นใช้ `RegularPrice`
- **L009**: กำหนด `oldPrice` → ถ้ามีลดให้เก็บราคาปกติไว้แสดงขีดทับ ไม่งั้นเป็น null
- **L010**: เก็บ `discount` จาก `Model.DiscountPercent`
- **L011**: บรรทัดว่าง
- **L012**: เริ่มเช็ค thumbnail
- **L013**: ถ้าไม่มีรูปจาก model ใช้ fallback `/images/Running-Shoe-2.jpeg`
- **L014**: ถ้ามีรูปจาก model ให้ใช้ค่านั้น
- **L015**: บรรทัดว่าง
- **L016**: สร้างลิสต์ `allImages` และใส่ thumbnail เป็นรูปแรก
- **L017**: เช็คว่ามี gallery urls หรือไม่
- **L018**: เปิดบล็อก if
- **L019**: มี gallery → append ทุกรูปเข้า `allImages`
- **L020**: ปิดบล็อก if
- **L021**: กรณีไม่มีก็เข้า else
- **L022**: เปิดบล็อก else
- **L023**: เพิ่มรูป fallback รูปที่ 2
- **L024**: เพิ่มรูป fallback รูปที่ 3
- **L025**: ปิดบล็อก else
- **L026**: บรรทัดว่าง
- **L027**: กำหนดชุดไซส์ EU สำหรับแสดงเริ่มต้น
- **L028**: กำหนดชุดไซส์ US สำหรับสลับด้วย JS
- **L029**: ปิด C# code block
- **L030**: บรรทัดว่าง
- **L031**: `@section Styles {` → เริ่ม section CSS ของหน้านี้
- **L032**: โหลดไฟล์ `CustomerProduct.css` พร้อม version append เพื่อกัน cache เก่า
- **L033**: ปิด section styles
- **L034**: บรรทัดว่าง
- **L035**: เปิด wrapper หลักของหน้า `.cp-page`
- **L036**: บรรทัดว่าง
- **L037**: คอมเมนต์ Breadcrumb
- **L038**: เปิดรายการ breadcrumb
- **L039**: breadcrumb รายการ Home
- **L040**: breadcrumb รายการ Category จาก model
- **L041**: breadcrumb รายการ Gender จาก model
- **L042**: breadcrumb รายการสุดท้ายเป็นชื่อสินค้า
- **L043**: ปิด ul breadcrumb
- **L044**: บรรทัดว่าง
- **L045**: คอมเมนต์แบ่งส่วน HERO
- **L046**: เปิดกริด hero (`ซ้ายรูป / ขวาข้อมูล`)
- **L047**: บรรทัดว่าง
- **L048**: คอมเมนต์ฝั่งซ้ายรูป
- **L049**: เปิด container รูปทั้งหมด
- **L050**: เปิดแถว thumbnail
- **L051**: ลูปสร้าง thumbnail ตามจำนวนรูปใน `allImages`
- **L052**: เปิดบล็อก for
- **L053**: `<img>` thumbnail; รูปแรกมี class `active`
- **L054**: กำหนด src ตาม index และ alt ตามลำดับรูป
- **L055**: ผูก click event ให้เรียก `setMainImage(this)`
- **L056**: ปิดบล็อก for
- **L057**: ปิด container thumbs
- **L058**: เปิด wrapper รูปหลัก
- **L059**: แสดงรูปหลัก (`id=mainImage`) เริ่มที่รูปแรก
- **L060**: เงื่อนไขแสดงป้ายส่วนลดบนรูปหลัก
- **L061**: เปิดบล็อก if ส่วนลด
- **L062**: แสดง badge ส่วนลดเป็นเปอร์เซ็นต์ format `0.##`
- **L063**: ปิดบล็อก if
- **L064**: ปิด wrapper รูปหลัก
- **L065**: ปิดฝั่งรูปซ้าย
- **L066**: บรรทัดว่าง
- **L067**: คอมเมนต์ฝั่งขวาข้อมูลสินค้า
- **L068**: เปิด container ข้อมูลขวา
- **L069**: แสดง brand และ category
- **L070**: แสดงชื่อสินค้าด้วย `<h1>`
- **L071**: บรรทัดว่าง
- **L072**: แสดงข้อความประเภทผู้ใช้ เช่น Men's/Women's/Unisex (ปัจจุบันใช้ `Gender's Shoes`)
- **L073**: บรรทัดว่าง
- **L074**: เปิดบล็อกราคา
- **L075**: แสดงราคาปัจจุบัน (`currentPrice`)
- **L076**: เงื่อนไขมี `oldPrice` หรือไม่
- **L077**: เปิด if
- **L078**: แสดงราคาปกติแบบขีดทับ
- **L079**: ปิด if
- **L080**: เงื่อนไขแสดงข้อความ `% off`
- **L081**: เปิด if
- **L082**: แสดงข้อความส่วนลด
- **L083**: ปิด if
- **L084**: ปิดบล็อกราคา
- **L085**: บรรทัดว่าง
- **L086**: คอมเมนต์ส่วน color swatches
- **L087**: เช็คว่ามีสีหรือไม่ (`ColorNames`)
- **L088**: เปิด if
- **L089**: เปิด section สี
- **L090**: แสดง label สี + แสดงสีที่เลือกปัจจุบันใน `#selectedColor`
- **L091**: เปิด container ปุ่มสี
- **L092**: ลูปทุกสีใน model
- **L093**: เปิด foreach
- **L094**: เริ่มปุ่มแต่ละสี
- **L095**: ถ้าสีนั้นเท่ากับตัวแรกให้ active
- **L096**: คลิกแล้วเรียก `pickColor(this, 'ชื่อสี')`
- **L097**: แสดงชื่อสีบนปุ่ม
- **L098**: ปิดปุ่ม
- **L099**: ปิด foreach
- **L100**: ปิด container swatches
- **L101**: ปิด section สี
- **L102**: ปิด if สี
- **L103**: บรรทัดว่าง
- **L104**: คอมเมนต์ส่วน size grid
- **L105**: เปิด section ไซส์
- **L106**: เปิดหัว section ไซส์
- **L107**: label “Select Size”
- **L108**: ข้อความ “Size Guide” (ตอนนี้เป็นข้อความเฉย ๆ)
- **L109**: ปิดหัว section
- **L110**: บรรทัดว่าง
- **L111**: เปิดแท็บชนิดไซส์ (EU/US)
- **L112**: ปุ่มแท็บ EU เป็น active เริ่มต้น + click สลับไซส์
- **L113**: ปุ่มแท็บ US + click สลับไซส์
- **L114**: ปิดแท็บไซส์
- **L115**: บรรทัดว่าง
- **L116**: เปิดกริดปุ่มไซส์ `id=sizeGrid`
- **L117**: ลูปไซส์ EU เป็นค่าเริ่มต้น
- **L118**: เปิด foreach
- **L119**: สร้างปุ่มไซส์แต่ละค่า + click `pickSize(this)`
- **L120**: ปิด foreach
- **L121**: ปิดกริดไซส์
- **L122**: ปิด section ไซส์
- **L123**: บรรทัดว่าง
- **L124**: คอมเมนต์ส่วน quantity
- **L125**: เปิด section quantity
- **L126**: แสดง label Quantity
- **L127**: ปุ่มลดจำนวน เรียก `changeQty(-1)`
- **L128**: input จำนวนแบบ readonly (ค่าเริ่มต้น 1)
- **L129**: ปุ่มเพิ่มจำนวน เรียก `changeQty(1)`
- **L130**: ปิด section quantity
- **L131**: บรรทัดว่าง
- **L132**: คอมเมนต์ส่วน CTA
- **L133**: เปิด container ปุ่ม action
- **L134**: ปุ่ม `Add to Bag` (ตอนนี้ยัง mock)
- **L135**: ปุ่ม Favorite
- **L136**: ไอคอนหัวใจจาก bootstrap-icons + ข้อความ Favorite
- **L137**: ปิดปุ่ม favorite
- **L138**: ปิด container actions
- **L139**: บรรทัดว่าง
- **L140**: เปิดข้อความสถานะ stock/shipping
- **L141**: แสดงจุดสี + ข้อความ In stock และเงื่อนไขส่งฟรี
- **L142**: ปิดข้อความ stock
- **L143**: บรรทัดว่าง
- **L144**: คอมเมนต์ short description
- **L145**: เปิด div แสดงคำอธิบายสั้น พร้อม inline style
- **L146**: แสดง `Model.ShortDescription`
- **L147**: ปิด div
- **L148**: ปิด container ข้อมูลขวา
- **L149**: บรรทัดว่าง
- **L150**: ปิด hero grid
- **L151**: คอมเมนต์ปิด hero
- **L152**: บรรทัดว่าง
- **L153**: คอมเมนต์ส่วน detail cards ด้านล่าง
- **L154**: เปิด container รายละเอียด
- **L155**: บรรทัดว่าง
- **L156**: เงื่อนไข ถ้ามี `FullDescription`
- **L157**: เปิด if
- **L158**: เปิดการ์ดรายละเอียด
- **L159**: หัวข้อ Description
- **L160**: แสดง `Model.FullDescription`
- **L161**: ปิดการ์ด
- **L162**: ปิด if
- **L163**: บรรทัดว่าง
- **L164**: เงื่อนไข ถ้ามี `HighlightFeaturesHtml`
- **L165**: เปิด if
- **L166**: เปิดการ์ดรายละเอียด
- **L167**: หัวข้อ Highlights
- **L168**: แสดง HTML ตรง ๆ ด้วย `@Html.Raw(...)`
- **L169**: ปิดการ์ด
- **L170**: ปิด if
- **L171**: บรรทัดว่าง
- **L172**: เงื่อนไข ถ้ามี `TechnicalDetails`
- **L173**: เปิด if
- **L174**: เปิดการ์ดรายละเอียด
- **L175**: หัวข้อ Technical Details
- **L176**: เปิด container ชิปสเปค
- **L177**: ลูปตัดข้อความสเปคด้วย `;` และทิ้งช่องว่างว่างเปล่า
- **L178**: เปิด foreach
- **L179**: แสดงแต่ละ item เป็นชิป
- **L180**: ปิด foreach
- **L181**: ปิด container specs
- **L182**: ปิดการ์ด
- **L183**: ปิด if
- **L184**: บรรทัดว่าง
- **L185**: ปิด container รายละเอียด
- **L186**: ปิด wrapper หลักของหน้า
- **L187**: บรรทัดว่าง
- **L188**: เริ่ม `@section Scripts` ของหน้านี้
- **L189**: เปิดแท็ก `<script>`
- **L190**: เปิด IIFE `(function(){ ... })();` เพื่อไม่ให้ชน global เกินจำเป็น
- **L191**: คอมเมนต์ฟังก์ชัน thumbnail → main image
- **L192**: ประกาศ `window.setMainImage = function(thumb){...}`
- **L193**: เปลี่ยน src ของรูปหลักเป็น src ของ thumbnail ที่กด
- **L194**: ลบ class active จาก thumbnail ทุกตัว
- **L195**: ใส่ active ให้ thumbnail ที่เลือก
- **L196**: ปิดฟังก์ชัน
- **L197**: บรรทัดว่าง
- **L198**: คอมเมนต์ฟังก์ชันเลือกสี
- **L199**: ประกาศ `window.pickColor = function(btn,name){...}`
- **L200**: ลบ active จาก swatch ทุกตัว
- **L201**: ใส่ active ให้ swatch ที่กด
- **L202**: อัปเดตข้อความสีที่เลือกใน `#selectedColor`
- **L203**: ปิดฟังก์ชัน
- **L204**: บรรทัดว่าง
- **L205**: คอมเมนต์ส่วนสลับประเภทไซส์
- **L206**: ส่ง `euSizes` จาก C# ไป JavaScript เป็น JSON array
- **L207**: ส่ง `usSizes` จาก C# ไป JavaScript เป็น JSON array
- **L208**: บรรทัดว่าง
- **L209**: ประกาศ `window.switchSizeType = function(tab,type){...}`
- **L210**: ลบ active จากแท็บ EU/US ทั้งหมด
- **L211**: ใส่ active ให้แท็บที่กด
- **L212**: บรรทัดว่าง
- **L213**: หา DOM ของ `sizeGrid`
- **L214**: เลือกชุดไซส์ตามชนิด (`US` หรือ `EU`)
- **L215**: สร้าง HTML ปุ่มไซส์ใหม่จาก array ด้วย `map`
- **L216**: template string ของปุ่มไซส์ + onclick `pickSize(this)`
- **L217**: รวม string ด้วย `.join('')` แล้วใส่กลับ `grid.innerHTML`
- **L218**: ปิดฟังก์ชัน switchSizeType
- **L219**: บรรทัดว่าง
- **L220**: คอมเมนต์ฟังก์ชันเลือกไซส์
- **L221**: ประกาศ `window.pickSize = function(btn){...}`
- **L222**: ถ้าปุ่มมี class `disabled` ให้ออกจากฟังก์ชันทันที
- **L223**: ลบ active จากปุ่มไซส์ทั้งหมด
- **L224**: ใส่ active ให้ไซส์ที่กด
- **L225**: ปิดฟังก์ชัน
- **L226**: บรรทัดว่าง
- **L227**: คอมเมนต์ฟังก์ชัน quantity
- **L228**: ประกาศ `window.changeQty = function(d){...}`
- **L229**: ดึง input `#qtyValue`
- **L230**: แปลงค่าปัจจุบันเป็นเลข ถ้า parse ไม่ได้ให้ใช้ 1
- **L231**: บังคับขั้นต่ำเป็น 1 แล้วบวก/ลบตาม `d`
- **L232**: เขียนค่ากลับ input
- **L233**: ปิดฟังก์ชัน
- **L234**: ปิด IIFE
- **L235**: ปิด `<script>`
- **L236**: ปิด `@section Scripts`

---

## 4) ตัวอย่างการทำงานจริง (Scenario)

### ตัวอย่าง A: ราคาลด
- ถ้า `RegularPrice = 3200`, `SalePrice = 2990`, `DiscountPercent = 6.56`
- `hasSale` จะเป็น true
- หน้าแสดง:
  - `฿2,990` เป็นราคาปัจจุบัน
  - `฿3,200` แบบขีดทับ
  - badge/label `6.56% off`

### ตัวอย่าง B: รูปสินค้า
- ถ้า `ThumbnailUrl` ว่าง → ใช้ `/images/Running-Shoe-2.jpeg`
- ถ้า `GalleryUrls` ว่าง → เพิ่ม fallback 2 รูป
- เมื่อคลิก thumbnail ภาพหลัก (`#mainImage`) จะเปลี่ยนทันที

### ตัวอย่าง C: สลับไซส์ EU/US
- กดแท็บ `US` → เรียก `switchSizeType(this,'US')`
- JS จะ rebuild ปุ่มใน `#sizeGrid` เป็นชุด US (`6`..`11.5`)
- กดไซส์หนึ่งปุ่ม → active เฉพาะปุ่มนั้น

### ตัวอย่าง D: เลือกจำนวน
- ค่าเริ่มต้น quantity = `1`
- กด `+` จะเพิ่มทีละ 1
- กด `-` จะลดได้ แต่ต่ำสุดไม่ต่ำกว่า 1

---

## 5) จุดที่ควรรู้เพิ่มเติม

- หน้านี้ยังเป็น **mock UI**: ปุ่ม `Add to Bag` และ `Favorite` ยังไม่ยิง backend
- `@Html.Raw(Model.HighlightFeaturesHtml)` ต้องใช้กับข้อมูลที่ sanitize แล้วเท่านั้น
- Inline style บางจุด (เช่น L072/L145/L126) ถ้าต้องการ maintain ง่ายขึ้น ควรย้ายไป CSS ภายหลัง
