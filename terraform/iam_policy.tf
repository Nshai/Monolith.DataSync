data "aws_iam_policy_document" "queues" {
  # SQS
  statement {
    effect = "Allow"
    actions = [
      "sqs:ChangeMessageVisibility",
      "sqs:CreateQueue",
      "sqs:DeleteMessage",
      "sqs:GetQueueAttributes",
      "sqs:GetQueueUrl",
      "sqs:ReceiveMessage",
      "sqs:SendMessage",
      "sqs:SetQueueAttributes",
    ]
    resources = [
      "arn:aws:sqs:${var.aws_region}:${var.aws_account_id}:${var.message_resource}",
    ]
  }

  # SNS
  statement {
    effect = "Allow"
    actions = [
      "sns:Subscribe",
      "sns:CreateTopic",
      "sns:Publish"
    ]
    resources = [
      "arn:aws:sns:${var.aws_region}:${var.aws_account_id}:${var.message_resource}",
    ]
  }

  statement {
    effect = "Allow"
    actions = [
      "sqs:ListQueues",
      "sns:ListTopics",
      "sns:ListSubscriptionsByTopic",
      "sns:SetSubscriptionAttributes",
      "sns:SetTopicAttributes",
      "sns:GetTopicAttributes"
    ]
    resources = [
      "*"
    ]
  }
}