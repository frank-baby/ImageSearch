import { useEffect } from 'react';
import { HiX } from 'react-icons/hi';
import type { ProcessedImage } from '../../types/imageSearch.types';

interface ImagePreviewProps {
    image: ProcessedImage;
    onClose: () => void;
}

const ImagePreview = ({ image, onClose }: ImagePreviewProps) => {
    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Escape') {
                onClose();
            }
        };

        window.addEventListener('keydown', handleKeyDown);
        return () => window.removeEventListener('keydown', handleKeyDown);
    }, [onClose]);

    const handleBackdropClick = (e: React.MouseEvent) => {
        if (e.target === e.currentTarget) {
            onClose();
        }
    };

    return (
        <div
            className="fixed inset-0 z-50 bg-black/90 flex items-center justify-center p-4"
            onClick={handleBackdropClick}
            role="dialog"
            aria-modal="true"
            aria-labelledby="modal-title"
        >
            <button
                type="button"
                onClick={onClose}
                className="absolute top-4 right-4 text-white hover:text-gray-300 transition-colors z-10"
                aria-label="Close preview"
            >
                <HiX className="w-8 h-8" />
            </button>

            <div className="max-w-5xl max-h-[90vh] relative">
                <img
                    src={image.smallImageUrl || image.thumbnailUrl || ''}
                    alt={image.altDescription || image.description || 'Unsplash image'}
                    className="max-w-full max-h-[90vh] object-contain rounded-lg"
                />

                {(image.altDescription || image.description) && (
                    <div className="absolute bottom-0 left-0 right-0 bg-linear-to-t from-black to-transparent p-6 rounded-b-lg">
                        <p id="modal-title" className="text-white text-lg">
                            {image.altDescription || image.description}
                        </p>
                    </div>
                )}
            </div>

            <p className="absolute bottom-4 text-white text-sm opacity-75">
                Click outside or press ESC to exit
            </p>
        </div>
    );
};

export default ImagePreview;
