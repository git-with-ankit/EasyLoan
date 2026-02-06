/**
 * Generic pagination models for server-side pagination
 */

/**
 * Pagination parameters for API requests
 */
export interface PaginationParams {
    pageNumber: number;  // 1-indexed for backend
    pageSize: number;
}

/**
 * Generic paginated response from the server
 */
export interface PagedResponse<T> {
    items: T[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}

/**
 * Helper function to create pagination params
 */
export function createPaginationParams(pageNumber: number = 1, pageSize: number = 10): PaginationParams {
    return { pageNumber, pageSize };
}
