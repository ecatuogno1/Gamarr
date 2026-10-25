import React from 'react';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import GameIndexTags from 'Game/Index/GameIndexTags';

interface TagsCellProps {
  className: string;
  tags: number[];
}

function TagsCell({ className, tags }: TagsCellProps) {
  return (
    <VirtualTableRowCell className={className}>
      <GameIndexTags tags={tags} />
    </VirtualTableRowCell>
  );
}

export default TagsCell;
