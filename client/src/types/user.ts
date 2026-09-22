export type User = {
    id: string;
    displayName: string;
    email: string;
    token: string;
    imageUrl?: string;
    roles: string[];
}

export type ManagedUser = {
    id: string;
    email: string;
    displayName: string;
    imageUrl?: string;
    roles: string[];
    houseNumber?: string;
    zone?: string;
    barangay?: string;
    city?: string;
    province?: string;
    phoneNumber?: string;
}

export type ManagedUserRequest = {
    email: string;
    password?: string;
    displayName: string;
    imageUrl?: string;
    roles: string[];
    houseNumber?: string;
    zone?: string;
    barangay: string;
    city: string;
    province: string;
    phoneNumber: string;
}

export type LoginCreds = {
    email: string;
    password: string;
}

export type RegisterCreds = {
    email: string;
    displayName: string;
    password: string;
    gender: string;
    dateOfBirth: string;
    city: string;
    country: string;
}

export type Role = {
    id: string;
    name: string;
    description: string | null;
}

export type Roles = Role[];