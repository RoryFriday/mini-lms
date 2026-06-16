# Deploy the App & Provide a Live URL

## What
Deploy the Library Management System to AWS and expose it via a public URL for live testing.

## How

### 1. Prerequisites
- AWS account with appropriate IAM permissions
- AWS CLI configured locally (`aws configure`)
- Terraform >= 1.0 installed
- Docker installed

### 2. Infrastructure Provisioning
```bash
cd infra

# Create the S3 bucket for Terraform state (one-time)
aws s3 mb s3://lms-terraform-state --region us-east-1

# Initialize and apply
terraform init
terraform plan -out=plan.tfplan
terraform apply plan.tfplan
```
This creates: VPC, ECS Fargate cluster, ALB, ECR repos, security groups, CloudWatch logs.

### 3. Build & Push Docker Images
```bash
# Authenticate Docker to ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-east-1.amazonaws.com

# Build and push backend
cd backend
docker build -t lms-backend .
docker tag lms-backend:latest <backend-ecr-url>:latest
docker push <backend-ecr-url>:latest

# Build frontend (with ALB DNS as API base URL)
cd ../frontend
docker build --build-arg REACT_APP_API_URL=http://<alb-dns-name> -t lms-frontend .
docker tag lms-frontend:latest <frontend-ecr-url>:latest
docker push <frontend-ecr-url>:latest
```

### 4. Force ECS Service Update
```bash
aws ecs update-service --cluster lms-cluster --service lms-backend --force-new-deployment
aws ecs update-service --cluster lms-cluster --service lms-frontend --force-new-deployment
```

### 5. Access the App
The live URL will be the ALB DNS name output from Terraform:
```
http://<alb-dns-name>
```

### Optional Enhancements for Production
- Add Route 53 custom domain + ACM certificate for HTTPS
- Add an RDS PostgreSQL instance instead of SQLite for data durability
- Add EFS mount for SQLite persistence across Fargate task restarts
- Set up CI/CD pipeline with GitHub Actions or AWS CodePipeline
- Enable auto-scaling policies on ECS services
