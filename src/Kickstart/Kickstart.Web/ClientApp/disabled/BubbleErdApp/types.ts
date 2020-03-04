export namespace d3Types {
  export type d3Node = {
      id: string,
      label: string,
      numberOfChildren: number,
       group: number;
  };

  export type d3Link = {
    source: string,
    target: string,
    value: number;
  };

  export type d3Graph = {
     nodes : d3Node[],
    links: d3Link[];
  };
}
