// shell.d.ts
declare global {
    interface Shell {
        ajaxSafePost<TResponse>(options: AjaxSafePostOptions): JQueryPromise<TResponse>;
    }

    interface AjaxSafePostOptions {
        type: string;
        url: string;
        data: string;
        headers: Record<string, string>;
    }

    const shell: Shell;
}

export {};
