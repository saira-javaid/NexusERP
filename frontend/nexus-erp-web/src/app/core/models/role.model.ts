export interface Role {
  id: string;
  name: string;
  description?: string;
  userCount: number;
  permissionCount: number;
  createdAt: string;
}

export interface RoleDetail {
  id: string;
  name: string;
  description?: string;
  permissions: string[];
  permissionIds: string[];
  createdAt: string;
}

export interface Permission {
  id: string;
  name: string;
  module: string;
  description?: string;
}

export interface CreateRoleRequest {
  name: string;
  description?: string;
}

export interface UpdateRoleRequest {
  id: string;
  name: string;
  description?: string;
}

export interface UpdateRolePermissionsRequest {
  permissionIds: string[];
}
