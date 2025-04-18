# PortScanner

**PortScanner** is a lightweight and fast TCP port scanner written in C#. It supports scanning individual IPs, multiple IPs, or full subnets (CIDR), with support for port ranges and optional output to a file.

---

## ✅ Features

- 🧠 **Simple usage**: Just pass target IPs and ports
- 🌐 **CIDR support**: Scan entire subnets like `192.168.1.0/24`
- 🔁 **Port range or list**: `22,80,443` or `1-1000`
- ⚡ **Multithreaded scanning**
- 📂 **Optional output to file**
- ⏱️ **Custom timeout** (default: 500ms)

---

## 🚀 Usage

```bash
PortScanner.exe <hosts> <ports> [timeout_ms] [outfile]
```

| Argument     | Description                                  |
|--------------|----------------------------------------------|
| `hosts`      | Single IP, comma-separated list, or CIDR     |
| `ports`      | Comma-separated list and/or range (e.g. 80,443 or 1-1000) |
| `timeout_ms` | (Optional) Timeout per connection in ms (default: 500) |
| `outfile`    | (Optional) Write results to file             |

---

## 🔧 Examples

```bash
# Scan one IP for ports 80, 443, and 8080
PortScanner.exe 192.168.1.10 80,443,8080

# Scan full subnet for ports 1 to 1000
PortScanner.exe 192.168.1.0/24 1-1000

# Scan two IPs with custom timeout and save results
PortScanner.exe 192.168.1.10,192.168.1.20 22,80 1000 results.txt
```

---

## Requirements

- C# 7.3 compatible (no .NET Core required)
- .NET Framework project (no async/await or modern features)

---


## Disclaimer

This tool is intended for **authorized testing and educational purposes only**. Always ensure you have permission to scan the targets.

---

## Author

**Bob Builder**  
*Built with simplicity and performance in mind.*  
```
