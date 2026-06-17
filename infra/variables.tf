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

variable "ai_provider" {
  description = "AI provider to use: OpenAi or Anthropic"
  type        = string
  default     = "OpenAi"
}

variable "openai_api_key" {
  description = "OpenAI API key"
  type        = string
  sensitive   = true
  default     = ""
}

variable "openai_model" {
  description = "OpenAI model to use"
  type        = string
  default     = "gpt-4o"
}

variable "anthropic_api_key" {
  description = "Anthropic API key"
  type        = string
  sensitive   = true
  default     = ""
}

variable "anthropic_model" {
  description = "Anthropic model to use"
  type        = string
  default     = "claude-sonnet-4-20250514"
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
