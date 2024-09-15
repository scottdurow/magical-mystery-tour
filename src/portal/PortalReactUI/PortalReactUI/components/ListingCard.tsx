import * as React from 'react';
import { Button, Card } from 'react-bootstrap';
import { contoso_listing } from '../dataverse-gen/entities/contoso_listing';
import { contoso_ListingImageAttributes, contoso_listingimageMetadata } from '../dataverse-gen';
import { ImageLoader } from './ImageLoader';

interface ListingCardProps {
    listing: contoso_listing;
}

const ListingCard: React.FC<ListingCardProps> = ({ listing }) => {
    const imageDownloadUrl =
        listing.contoso_primaryimage &&
        `/Image/download.aspx?entity=${contoso_listingimageMetadata.logicalName}&attribute=${contoso_ListingImageAttributes.contoso_Image}&ID=${listing.contoso_primaryimage.id}&Full=true`;
    const listingUrl = `/Listing-Details-PCF/?id=${listing.contoso_listingid}`;
    return (
        <Card
            style={{ width: '18rem', height: 'auto' }}
            className={'bg-light text-black text-center p-0 overflow-hidden shadow mx-auto mb-4'}
        >
            {imageDownloadUrl && (
                <a href={listingUrl}>
                    <ImageLoader
                        imageUrl={imageDownloadUrl}
                        className="card-img-top img-fluid  w-100"
                        style={{ height: '15rem', objectFit: 'cover', objectPosition: 'center center' }}
                    />
                    {/* <Card.Img variant="top" src={imageDownloadUrl} style={{height: '15rem', objectFit: 'cover', objectPosition: 'center'}} className="img-fluid  w-100" /> */}
                </a>
            )}

            <Card.Body className="d-flex flex-column">
                <Card.Title style={{ textOverflow: 'ellipsis', overflow: 'hidden', whiteSpace: 'nowrap' }}>
                    {listing.contoso_name}
                </Card.Title>
                <Card.Text>{listing.contoso_description}</Card.Text>
                <div className="d-flex justify-content-between mt-auto">
                    <span style={{ whiteSpace: 'nowrap' }}>
                        {listing.contoso_pricepermonth?.toLocaleString('en-US', { style: 'currency', currency: 'USD' })}
                        /month
                    </span>
                    <Button
                        className="bg-light-primary d-flex align-item-center ml-auto border-0 px-3"
                        href={listingUrl}
                    >
                        View listing
                    </Button>
                </div>
            </Card.Body>
        </Card>
    );
};

export default ListingCard;
