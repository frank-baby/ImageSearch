import { HiZoomIn, HiPhotograph } from 'react-icons/hi';
import type { ProcessedImage } from '../../types/imageSearch.types';

interface ImageGridProps {
    images: ProcessedImage[];
    onImageClick: (image: ProcessedImage) => void;
}

const ImageGrid = ({ images, onImageClick }: ImageGridProps) => {
    return (
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
            {images.map((image, index) => (
                <div
                    key={`${image.imageId}-${index}`}
                    className="bg-white rounded-lg shadow-md overflow-hidden hover:shadow-xl transition-all duration-300 cursor-pointer group"
                    onClick={() => onImageClick(image)}
                >
                    <div className="aspect-square relative bg-gray-100 overflow-hidden">
                        {(image.thumbnailUrl || image.smallImageUrl) ? (
                            <>
                                <img
                                    src={image.thumbnailUrl || image.smallImageUrl || ''}
                                    alt={image.altDescription || image.description || 'Unsplash image'}
                                    className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-110"
                                    loading="lazy"
                                    onError={(e) => {
                                        e.currentTarget.style.display = 'none';
                                        e.currentTarget.parentElement!.classList.add('flex', 'items-center', 'justify-center');
                                    }}
                                />
                                <div className="absolute inset-0 bg-black/0 group-hover:bg-black/30 transition-all duration-300 flex items-center justify-center pointer-events-none">
                                    <HiZoomIn className="w-12 h-12 text-white opacity-0 group-hover:opacity-100 transition-opacity duration-300" />
                                </div>
                            </>
                        ) : (
                            <div className="flex items-center justify-center w-full h-full">
                                <HiPhotograph className="w-12 h-12 text-gray-400" />
                            </div>
                        )}
                    </div>
                    {(image.altDescription || image.description) && (
                        <div className="p-3">
                            <p className="text-sm text-gray-700 line-clamp-2">
                                {image.altDescription || image.description}
                            </p>
                        </div>
                    )}
                </div>
            ))}
        </div>
    );
};

export default ImageGrid;
