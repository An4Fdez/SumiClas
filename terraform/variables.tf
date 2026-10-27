variable "location" {
  description = "Azure region for every resource."
  type        = string
  default     = "westeurope"
}

variable "environment" {
  description = "Environment name, used in resource names and tags."
  type        = string
  default     = "prod"
}

variable "postgres_admin_password" {
  description = "Administrator password for PostgreSQL. Choose it yourself and reuse it in the connection string."
  type        = string
  sensitive   = true
}

variable "app_service_sku" {
  description = "App Service plan SKU."
  type        = string
  default     = "B1"
}

variable "postgres_sku" {
  description = "PostgreSQL Flexible Server SKU."
  type        = string
  default     = "B_Standard_B1ms"
}

variable "postgres_storage_mb" {
  description = "PostgreSQL storage in MB."
  type        = number
  default     = 32768
}
