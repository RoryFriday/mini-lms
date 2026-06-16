variable "aws_region" {
  description = "AWS region to deploy to"
  type        = string
  default     = "us-east-1"
}

variable "app_name" {
  description = "Application name used for resource naming"
  type        = string
  default     = "lms"
}

variable "jwt_secret" {
  description = "JWT signing secret key"
  type        = string
  sensitive   = true
  default     = "SuperSecretKeyThatIsLongEnough1234567890!"
}

variable "tags" {
  description = "Tags applied to all resources"
  type        = map(string)
  default = {
    Project     = "LibraryManagementSystem"
    Environment = "production"
    ManagedBy   = "terraform"
  }
}
