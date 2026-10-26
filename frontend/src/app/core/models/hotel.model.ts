export interface Hotel {
  id: number;
  name: string;
  city: string;
  address: string;
  description: string | null;
  rating: number;
  pricePerNight: number;
  imageUrl: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export type HotelRequest = Omit<Hotel, 'id' | 'createdAt' | 'updatedAt'>;

export interface HotelFilter {
  name?: string;
  /** Exact rating (1-5). */
  rating?: number | null;
  /** Minimum rating (1-5). */
  minRating?: number | null;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  username: string;
  role: string;
}

/** ProblemDetails error payload returned by the API. */
export interface ProblemDetails {
  title?: string;
  status?: number;
  errors?: Record<string, string[]>;
}
