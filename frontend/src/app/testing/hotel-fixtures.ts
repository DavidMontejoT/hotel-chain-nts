import { Hotel, PagedResult } from '../core/models/hotel.model';

export function hotel(overrides: Partial<Hotel> = {}): Hotel {
  return {
    id: 1,
    name: 'Aurora Cartagena',
    city: 'Cartagena',
    address: 'Calle 1',
    description: null,
    rating: 5,
    pricePerNight: 100000,
    imageUrl: null,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null,
    ...overrides,
  };
}

export function page(items: Hotel[], totalPages = 1): PagedResult<Hotel> {
  return { items, total: items.length, page: 1, pageSize: 20, totalPages };
}
