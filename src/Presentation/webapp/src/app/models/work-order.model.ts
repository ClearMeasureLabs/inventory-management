export interface WorkOrderResponse {
  workOrderId: string;
  title: string;
}

export interface CreateWorkOrderRequest {
  title: string;
}

export interface ValidationProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: { [key: string]: string[] };
}
