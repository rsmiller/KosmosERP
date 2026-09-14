import { BaseDto, DataCommand } from "./base-models";

export class NotificationDto extends BaseDto {
  user_id?: number;
  object_name?: string;
  object_id?: number;
  alert_text?: string;
  notified?: boolean;
  notification_read?: boolean;
}

export class NotificationCreateCommand extends DataCommand {
  user_id?: number;
  object_name?: string;
  object_id?: number;
  alert_text?: string;
  notified?: boolean;
  notification_read?: boolean;
}

export class NotificationEditCommand extends DataCommand {
  id?: number;
  user_id?: number;
  object_name?: string;
  object_id?: number;
  alert_text?: string;
  notified?: boolean;
  notification_read?: boolean;
}

export class NotificationListDto extends BaseDto {
  user_id?: number;
  object_name?: string;
  object_id?: number;
  alert_text?: string;
  notified?: boolean;
  notification_read?: boolean;
}

export class NotificationDeleteCommand extends DataCommand {
  id?: number;
}

export class NotificationFindCommand extends DataCommand {
  user_id?: number;
  object_name?: string;
  object_id?: number;
  alert_text?: string;
  notified?: boolean;
  notification_read?: boolean;
} 