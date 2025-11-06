# Hướng dẫn setup ngrok cho MoMo callback

## Bước 1: Tải và cài đặt ngrok
1. Truy cập https://ngrok.com/
2. Đăng ký tài khoản miễn phí
3. Tải ngrok về máy

## Bước 2: Chạy ngrok
```bash
ngrok http 7158
```

## Bước 3: Cập nhật appsettings.development.json
Thay thế URL localhost bằng URL ngrok được tạo:
```json
"MoMoSettings": {
  "RedirectUrl": "https://your-ngrok-url.ngrok.io/Payment/MomoReturn",
  "IpnUrl": "https://your-ngrok-url.ngrok.io/Payment/MomoCallback"
}
```

## Bước 4: Restart ứng dụng