export interface ContainerResponse {
  containerId: number;
  name: string;
  description: string;
}

export interface CreateContainerRequest {
  name: string;
  description: string;
}

export interface ValidationProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: { [key: string]: string[] };
}
