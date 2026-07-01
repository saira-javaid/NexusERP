export interface UserListItem {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  isActive: boolean;
  lastLoginAt?: string;
  roles: string[];
  createdAt: string;
}

export interface UserDetail extends UserListItem {
  avatarUrl?: string;
  permissions: string[];
}

export interface CreateUserRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  roles?: string[];
  isActive?: boolean;
}

export interface UpdateUserRequest {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  roles?: string[];
}
