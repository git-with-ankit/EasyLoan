
export interface PaginationParams {
    pageNumber: number; 
    pageSize: number;
}

export interface PagedResponse<T> {
    items: T[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}

export function createPaginationParams(pageNumber: number = 1, pageSize: number = 10): PaginationParams {
    return { pageNumber, pageSize };
}
