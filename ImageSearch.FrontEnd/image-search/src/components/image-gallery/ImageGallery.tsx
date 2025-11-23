import { useState } from 'react';
import ImagePreview from '../image-preview/ImagePreview';
import ImageGrid from './ImageGrid';
import EmptyState from './EmptyState';
import ProcessingWarnings from './ProcessingWarnings';
import type { SearchResponse, ProcessedImage } from '../../types/imageSearch.types';

interface ImageGalleryProps {
    searchResult: SearchResponse;
}

const ImageGallery = ({ searchResult }: ImageGalleryProps) => {
    const [selectedImage, setSelectedImage] = useState<ProcessedImage | null>(null);

    const handleImageClick = (image: ProcessedImage) => {
        setSelectedImage(image);
    };

    const handleCloseModal = () => {
        setSelectedImage(null);
    };

    if (!searchResult) {
        return null;
    }

    const successfulImages = searchResult.processedImages.filter(img => img.success);
    const failedImages = searchResult.processedImages.filter(img => !img.success && img.errorMessage);

    if (successfulImages.length === 0) {
        return <EmptyState query={searchResult.searchQuery} />;
    }

    return (
        <div className="container mx-auto px-4 py-8">
            <div className="mb-6">
                <h2 className="text-2xl font-bold text-gray-800">
                    Results for "{searchResult.searchQuery}"
                </h2>
                <p className="text-gray-600 mt-1">
                    {searchResult.totalProcessed} image{searchResult.totalProcessed !== 1 ? 's' : ''} processed successfully
                    {searchResult.totalFailed > 0 && ` (${searchResult.totalFailed} failed)`}
                </p>
            </div>

            <ImageGrid images={successfulImages} onImageClick={handleImageClick} />

            <ProcessingWarnings failedImages={failedImages} />

            {selectedImage && (
                <ImagePreview image={selectedImage} onClose={handleCloseModal} />
            )}
        </div>
    );
};

export default ImageGallery;
