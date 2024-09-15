export enum ApiErrorCodes {
    NETWORK_ERROR = 'NETWORK_ERROR',
    API_CHECKOUT_ERROR = 'API_CHECKOUT_ERROR',
    API_CHECKOUT_LISTING_UNAVAILABLE = 'API_CHECKOUT_LISTING_UNAVAILABLE',
}

export class CustomError extends Error {
    code: string;

    constructor(message: string, code: ApiErrorCodes) {
        super(message);
        this.code = code;
        this.name = this.constructor.name;
    }
}

// See https://learn.microsoft.com/en-us/power-pages/configure/web-api-http-requests-handle-errors#error-codes
export class NetworkError extends CustomError {
    constructor(message: string) {
        super(message, ApiErrorCodes.NETWORK_ERROR);
    }
}

export class ApiError extends CustomError {
    constructor(message: string, code: ApiErrorCodes) {
        super(message, code);
    }
}
