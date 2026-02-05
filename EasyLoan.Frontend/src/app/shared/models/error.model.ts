// Backend error response interfaces matching ASP.NET Core ProblemDetails

// Standard ProblemDetails from GlobalExceptionMiddleware
export interface ProblemDetails {
    type: string;
    title: string;
    status: number;
    detail: string;
    instance: string;
}

// ValidationProblemDetails from model validation (400 Bad Request)
export interface ValidationProblemDetails extends ProblemDetails {
    errors: {
        [field: string]: string[];
    };
}

export function extractErrorMessage(error: { error: string | ValidationProblemDetails | ProblemDetails }): string {
    let errorData: ValidationProblemDetails | ProblemDetails | string = error.error;

    // If error.error is a stringified JSON, parse it
    if (typeof errorData === 'string') {
        try {
            errorData = JSON.parse(errorData) as ValidationProblemDetails | ProblemDetails;
        } catch {
            // If parsing fails, it's just a plain string error - return it
            return errorData as string;
        }
    }

    // Now errorData is an object - check if it's ValidationProblemDetails (has 'errors' property)
    if ('errors' in errorData) {
        const messages: string[] = [];
        for (const field in (errorData as ValidationProblemDetails).errors) {
            messages.push(...(errorData as ValidationProblemDetails).errors[field]);
        }
        return messages.join(', ');
    }

    // Otherwise it's ProblemDetails (has 'detail' property)
    return (errorData as ProblemDetails).detail;
}