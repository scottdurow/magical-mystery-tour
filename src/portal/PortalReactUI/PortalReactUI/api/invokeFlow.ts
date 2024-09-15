export async function invokeFlow<TResponse>(
    url: string,
    parameters: Record<string, string | number | boolean>,
): Promise<TResponse> {
    const payload = {
        eventData: JSON.stringify(parameters),
    };

    const jQueryPromise = shell.ajaxSafePost<string>({
        type: 'POST',
        url,
        data: JSON.stringify(payload),
        headers: {
            'Content-Type': 'application/json',
        },
    });

    const response = await toNativePromise<string>(jQueryPromise);
    return JSON.parse(response) as TResponse;
}

async function toNativePromise<T>(jqueryPromise: JQueryPromise<T>): Promise<T> {
    return new Promise<T>((resolve, reject) => {
        jqueryPromise.done((r: T) => resolve(r)).fail((e) => reject(e));
    });
}
