export interface SearchResponse {
    searchQuery: string;
    totalNumber: number;
    totalProcessed: number;
    totalFailed: number;
    processedImages: ProcessedImage[];
}

export interface ProcessedImage {
    success: boolean;
    imageId: string;
    altDescription: string | null;
    smallImageUrl: string | null;
    thumbnailUrl: string | null;
    description: string | null;
    errorMessage: string | null;
}