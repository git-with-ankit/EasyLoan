
export interface ProblemDetails {
    type?: string;
    title: string;
    status: number;
    detail: string;
    instance?: string;
}

export interface ValidationProblemDetails extends ProblemDetails {
    errors: {
        [field: string]: string[];
    };
}

export interface HttpErrorResponse {
    error: ProblemDetails | ValidationProblemDetails | string;
    status: number;
    statusText: string;
    message: string;
}


export function isValidationProblemDetails(error: any): error is ValidationProblemDetails {
    return error && typeof error === 'object' && 'errors' in error && typeof error.errors === 'object';
}

export function isProblemDetails(error: any): error is ProblemDetails {
    return error && typeof error === 'object' && 'detail' in error && 'title' in error;
}