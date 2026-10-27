output "resource_group" {
  value = azurerm_resource_group.main.name
}

output "app_name" {
  value = azurerm_windows_web_app.main.name
}

output "app_url" {
  value = "https://${azurerm_windows_web_app.main.default_hostname}"
}

output "postgres_fqdn" {
  value = azurerm_postgresql_flexible_server.main.fqdn
}
