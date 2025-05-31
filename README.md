# 線上書城（Online BookStore）

## 專案簡介

本專案為一個線上書城網站，提供書籍瀏覽、購物車、訂單管理、公司管理、使用者註冊與權限管理等功能，支援一般消費者與公司帳戶，並整合 Stripe 金流付款。

## 主要功能

- 書籍瀏覽與分類查詢
- 商品詳細資訊與多張圖片
- 購物車管理
- 線上結帳（Stripe 金流整合）
- 訂單管理（訂單狀態、出貨、付款、取消等）
- 公司資料管理
- 使用者註冊、登入、角色權限（Admin/Employee/Company/Customer）
- 後台管理（商品、類別、公司、使用者、訂單）

## 技術棧

- ASP.NET Core MVC
- Entity Framework Core（Code First）
- SQL Server
- N-Tier 架構（分層：Models, DataAccess, Utility, Web）
- Razor Pages
- Stripe API
- Bootstrap 5
- jQuery, DataTables, Toastr

## 專案結構

- Bulky.Models：資料模型（如 Product, Category, Company, OrderHeader, OrderDetail, ApplicationUser 等）
- Bulky.DataAccess：資料存取層，包含 DbContext、Repository、資料庫初始化與遷移
- Bulky.Utility：共用常數、工具類別、Email 發送、Stripe 設定
- BulkyWeb：主要網站專案，包含 MVC 控制器、View、靜態資源、區域（Areas）
- BulkyWebRazor_Temp：Razor Pages 範例專案

## 安裝與執行

1. **安裝相依套件**
   - 於專案根目錄執行：
     ```powershell
     dotnet restore
     ```
2. **設定資料庫連線**
   - 修改 `BulkyWeb/appsettings.json` 內的 `DefaultConnection` 為你的 SQL Server 連線字串。
3. **套用資料庫遷移與初始化**
   - 執行：
     ```powershell
     dotnet ef database update --project Bulky.DataAccess --startup-project BulkyWeb
     ```
   - 首次啟動會自動建立角色與管理員帳號（帳號：Admin@gmail.com，密碼：Admin123+）。
4. **啟動網站**
   - 執行：
     ```powershell
     dotnet run --project BulkyWeb
     ```
   - 預設網址：http://localhost:5000

## Stripe 金流測試卡號

- 卡號：4242-4242-4242-4242
- 有效日期、CVC 可任意填寫

## 預設管理員帳號

- 帳號：Admin@gmail.com
- 密碼：Admin123+

## 其他

- 若需新增商品圖片，請於商品編輯頁面上傳。
- 公司帳戶下單可享有先出貨後付款（30 天內付款）之權限。

---

如有問題請聯絡專案管理員。
