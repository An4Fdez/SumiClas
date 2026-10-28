locals {
  name_prefix = "sumiclas-${var.environment}"
  database    = "sumiclas"
  db_user     = "sumiclas"

  # Minutes an unhealthy instance waits before it is taken out of rotation (the provider accepts 2 to 10).
  health_check_eviction_minutes = 10

  tags = {
    app         = "sumiclas"
    environment = var.environment
    managed_by  = "terraform"
  }
}

resource "random_string" "suffix" {
  length  = 5
  upper   = false
  special = false
}

# The resource group already exists and is not managed here, so `terraform destroy` never deletes it.
data "azurerm_resource_group" "main" {
  name = var.resource_group_name
}

resource "azurerm_postgresql_flexible_server" "main" {
  name                          = "psql-${local.name_prefix}-${random_string.suffix.result}"
  resource_group_name           = data.azurerm_resource_group.main.name
  location                      = data.azurerm_resource_group.main.location
  version                       = "17"
  sku_name                      = var.postgres_sku
  storage_mb                    = var.postgres_storage_mb
  administrator_login           = local.db_user
  administrator_password        = var.postgres_admin_password
  public_network_access_enabled = true
  zone                          = "1"
  tags                          = local.tags

  lifecycle {
    ignore_changes = [zone]
  }
}

resource "azurerm_postgresql_flexible_server_database" "main" {
  name      = local.database
  server_id = azurerm_postgresql_flexible_server.main.id
  collation = "en_US.utf8"
  charset   = "UTF8"
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_postgresql_flexible_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_service_plan" "main" {
  name                = "asp-${local.name_prefix}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  os_type             = "Windows"
  sku_name            = var.app_service_sku
  tags                = local.tags
}

resource "azurerm_windows_web_app" "main" {
  name                = "app-${local.name_prefix}-${random_string.suffix.result}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id
  https_only          = true
  tags                = local.tags

  site_config {
    always_on           = true
    http2_enabled       = true
    minimum_tls_version = "1.2"
    ftps_state          = "Disabled"
    health_check_path   = "/healthy"

    health_check_eviction_time_in_min = local.health_check_eviction_minutes

    application_stack {
      current_stack  = "dotnet"
      dotnet_version = "v10.0"
    }
  }

  # App settings are configured by hand in the App Service; Terraform must not overwrite them.
  lifecycle {
    ignore_changes = [app_settings]
  }
}
