export class ClientBase {

    protected async transformOptions(options_: RequestInit): Promise<RequestInit> {
        return options_;
    }

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    protected async transformResult(url: string, response: Response, processor: (_response: Response) => Promise<any>): Promise<any> {
        return processor(response);
    }
}