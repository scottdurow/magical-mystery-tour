import * as React from 'react';
import { useState, useEffect } from 'react';

interface ImageLoaderProps extends React.ImgHTMLAttributes<HTMLImageElement> {
    imageUrl: string;
}
export const ImageLoader = ({ imageUrl, ...imgProps }: ImageLoaderProps) => {
    const [loading, setLoading] = useState(true);
    const [imageSrc, setImageSrc] = useState<string>();

    useEffect(() => {
        const loadImage = async () => {
            try {
                const response = await fetch(imageUrl, { cache: 'force-cache' });
                const blob = await response.blob();
                const objectURL = URL.createObjectURL(blob);
                setImageSrc(objectURL);
            } catch (error) {
                console.error('Failed to load image:', error);
            } finally {
                setLoading(false);
            }
        };

        loadImage();
    }, [imageUrl]);

    if (loading) {
        return (
            <span className="placeholder-glow">
                <span className={imgProps.className + ' placeholder'} style={imgProps.style}></span>
            </span>
        );
    }

    return <img src={imageSrc} {...imgProps} />;
};
