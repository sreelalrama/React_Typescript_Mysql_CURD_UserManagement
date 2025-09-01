export interface User {
  id?: number;
  name: string;
  email: string;
  age: number;
  created_at?: Date;
  updated_at?: Date;
}

export interface CreateUserRequest {
  name: string;
  email: string;
  age: number;
}

export interface UpdateUserRequest {
  name?: string;
  email?: string;
  age?: number;
}