
export interface ImageScale {
    data: string; // Base64 encoded string from C# byte[]
    mimeType: string;
    minTemperature?: number;
    maxTemperature?: number;
}
